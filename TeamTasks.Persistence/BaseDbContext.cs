using System.Reflection;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;
using TeamTasks.Application.Core.Abstractions;
using TeamTasks.Application.Core.Abstractions.Events;
using TeamTasks.Domain.Common.Core.Abstractions;
using TeamTasks.Domain.Common.Core.Primitives;
using TeamTasks.Domain.Common.Core.Primitives.Maybe;
using TeamTasks.Domain.Core.Events;
using TeamTasks.Domain.Core.Extensions;
using TeamTasks.Domain.Core.Primitives;
using TeamTasks.Identity.Domain.Entities;
using TeamTasks.Identity.Domain.Enumerations;

namespace TeamTasks.Persistence;

/// <summary>
/// Represents the application database context base class.
/// </summary>
public class BaseDbContext
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IDbContext
{
    private readonly IPublisher _mediator = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    /// <param name="mediator">The mediator.</param>
    public BaseDbContext(
        DbContextOptions<BaseDbContext> options,
        IPublisher mediator)
        : base(options)
    {
        _mediator = mediator;
    }

    /// <param name="dbContextOptions"></param>
    /// <inheritdoc />
    public BaseDbContext(DbContextOptions<BaseDbContext> dbContextOptions)
        : base(dbContextOptions) { }

    /// <inheritdoc />
    public BaseDbContext() { }
    
    /// <inheritdoc />
    public DatabaseFacade EfDatabase => Database;

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.ForeignKeyPropertiesMappedToUnrelatedTables));
        optionsBuilder.UseNpgsql("Server=localhost;Port=5433;Database=TTGenericDb;User Id=postgres;Password=1111;");
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.HasDefaultSchema("dbo");

        modelBuilder.Entity<IdentityUserLogin<Guid>>()
           .HasKey(l => new { l.LoginProvider, l.ProviderKey });

        modelBuilder.Entity<IdentityUserRole<Guid>>()
             .HasKey(l => new { l.UserId, l.RoleId });

        modelBuilder.Entity<IdentityUserToken<Guid>>()
            .HasKey(l => new { l.UserId, l.LoginProvider, l.Name });

        modelBuilder.Entity<Category>()
            .HasNoKey();
    }

    /// <inheritdoc />
    public new DbSet<TEntity> Set<TEntity>()
        where TEntity : class
        => base.Set<TEntity>();

    /// <exception cref="ArgumentNullException"></exception>
    /// <inheritdoc />
    public async Task<Maybe<TEntity>> GetByIdAsync<TEntity>(Guid id)
        where TEntity : Entity
        => id == Guid.Empty ?
            Maybe<TEntity>.None :
            Maybe<TEntity>.From(await Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id) 
            ?? throw new ArgumentNullException());

    /// <inheritdoc />
    public async System.Threading.Tasks.Task Insert<TEntity>(TEntity entity)
        where TEntity : Entity
        => await Set<TEntity>().AddAsync(entity);

    /// <inheritdoc />
    public async System.Threading.Tasks.Task InsertRange<TEntity>(IReadOnlyCollection<TEntity> entities)
        where TEntity : Entity
        => await Set<TEntity>().AddRangeAsync(entities);

    /// <inheritdoc />
    public new async Task Remove<TEntity>(TEntity entity)
        where TEntity : Entity
        => await Set<TEntity>()
            .WhereIf(
                entity is not null, 
                e => e.Id == entity!.Id)
            .ExecuteDeleteAsync();
    
    /// <inheritdoc />
    public Task<int> ExecuteSqlAsync(string sql, IEnumerable<NpgsqlParameter> parameters, CancellationToken cancellationToken = default)
        => Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
    
   /// <summary>
   /// Saves all of the pending changes in the unit of work.
   /// </summary>
   /// <param name="cancellationToken">The cancellation token.</param>
   /// <returns>The number of entities that have been saved.</returns>
   public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
   {
       DateTime utcNow = DateTime.UtcNow;

       UpdateAuditableEntities(utcNow);

       UpdateSoftDeletableEntities(utcNow);

       await PublishDomainEvents(cancellationToken);
       await PublishDomainEventsForIdentity(cancellationToken);

       return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates the entities implementing <see cref="IAuditableEntity"/> interface.
    /// </summary>
    /// <param name="utcNow">The current date and time in UTC format.</param>
    private void UpdateAuditableEntities(DateTime utcNow)
    {
        foreach (EntityEntry<IAuditableEntity> entityEntry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entityEntry.State == EntityState.Added)
            {
                entityEntry.Property(nameof(IAuditableEntity.CreatedOnUtc)).CurrentValue = utcNow;
            }

            if (entityEntry.State == EntityState.Modified)
            {
                entityEntry.Property(nameof(IAuditableEntity.ModifiedOnUtc)).CurrentValue = utcNow;
            }
        }
    }

        /// <summary>
        /// Updates the entities implementing <see cref="ISoftDeletableEntity"/> interface.
        /// </summary>
        /// <param name="utcNow">The current date and time in UTC format.</param>
        private void UpdateSoftDeletableEntities(DateTime utcNow)
        {
            foreach (EntityEntry<ISoftDeletableEntity> entityEntry in ChangeTracker.Entries<ISoftDeletableEntity>())
            {
                if (entityEntry.State != EntityState.Deleted)
                {
                    continue;
                }

                entityEntry.Property(nameof(ISoftDeletableEntity.DeletedOnUtc)).CurrentValue = utcNow;

                entityEntry.Property(nameof(ISoftDeletableEntity.Deleted)).CurrentValue = true;

                entityEntry.State = EntityState.Modified;

                UpdateDeletedEntityEntryReferencesToUnchanged(entityEntry);
            }
        }

        /// <summary>
        /// Updates the specified entity entry's referenced entries in the deleted state to the modified state.
        /// This method is recursive.
        /// </summary>
        /// <param name="entityEntry">The entity entry.</param>
        private static void UpdateDeletedEntityEntryReferencesToUnchanged(EntityEntry entityEntry)
        {
            if (!entityEntry.References.Any())
            {
                return;
            }

            foreach (ReferenceEntry referenceEntry in entityEntry.References
                         .Where(r => r.TargetEntry!.State == EntityState.Deleted))
            {
                if (referenceEntry.TargetEntry != null)
                {
                    referenceEntry.TargetEntry.State = EntityState.Unchanged;

                    UpdateDeletedEntityEntryReferencesToUnchanged(referenceEntry.TargetEntry);
                }
            }
        }
        
        /// <summary>
        /// Publishes and then clears all the domain events that exist within the current transaction.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task PublishDomainEventsForIdentity(CancellationToken cancellationToken)
        {
            List<EntityEntry<User>> aggregateRoots = ChangeTracker
                .Entries<User>()
                .Where(entityEntry => entityEntry.Entity.DomainEvents.Count is not 0)
                .ToList();

            List<IDomainEvent> domainEvents = aggregateRoots
                .SelectMany(entityEntry => entityEntry.Entity.DomainEvents).ToList();

            aggregateRoots.ForEach(entityEntry => entityEntry.Entity.ClearDomainEvents());

            IEnumerable<System.Threading.Tasks.Task> tasks = domainEvents.Select(async domainEvent => 
                await _mediator.Publish(domainEvent, cancellationToken));

            await System.Threading.Tasks.Task.WhenAll(tasks);
        }

        /// <summary>
        /// Publishes and then clears all the domain events that exist within the current transaction.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async System.Threading.Tasks.Task PublishDomainEvents(CancellationToken cancellationToken)
        {
            List<EntityEntry<AggregateRoot>> aggregateRoots = ChangeTracker
                .Entries<AggregateRoot>()
                .Where(entityEntry => entityEntry.Entity.DomainEvents.Count is not 0)
                .ToList();
            
            List<IDomainEvent> domainEvents = aggregateRoots
                .SelectMany(entityEntry => entityEntry.Entity.DomainEvents).ToList();

            aggregateRoots.ForEach(entityEntry => entityEntry.Entity.ClearDomainEvents());

            IEnumerable<System.Threading.Tasks.Task> tasks = domainEvents.Select(async domainEvent => 
                await _mediator.Publish(domainEvent, cancellationToken));

            await System.Threading.Tasks.Task.WhenAll(tasks);
        }

        /// <inheritdoc cref="FormattableString" />
        public async Task<int> ExecuteSqlAsync(FormattableString sql) =>
            await Database.ExecuteSqlAsync(sql);
}