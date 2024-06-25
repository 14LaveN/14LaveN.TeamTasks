using Microsoft.EntityFrameworkCore;
using TeamTasks.Identity.Domain.Repositories;
using TeamTasks.Persistence;
using TeamTasks.Domain.Common.Core.Primitives.Maybe;
using TeamTasks.Domain.Common.ValueObjects;
using TeamTasks.Identity.Domain.Entities;
using TeamTasks.Persistence;

namespace TeamTasks.Identity.Persistence.Repositories;

/// <summary>
/// Represents the user repository class.
/// </summary>
/// <param name="userDbContext">The user database context.</param>
public class UserRepository(BaseDbContext userDbContext)
    : IUserRepository
{
    /// <inheritdoc />
    public async Task<Maybe<User>> GetByIdAsync(Guid userId) =>
            await userDbContext
                .Set<User>()
                .AsNoTracking()
                .AsSplitQuery()
                .FirstOrDefaultAsync(x=>x.Id == userId) 
            ?? throw new ArgumentNullException();

    /// <inheritdoc />
    public async Task<Maybe<User>> GetByNameAsync(string name) =>
        await userDbContext
            .Set<User>()
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(x=>x.UserName == name) 
        ?? throw new ArgumentNullException();

    /// <inheritdoc />
    public async Task<Maybe<User>> GetByEmailAsync(EmailAddress emailAddress) =>
        await userDbContext
            .Set<User>()
            .FirstOrDefaultAsync(x=>x.EmailAddress == emailAddress) 
        ?? throw new ArgumentNullException();
}