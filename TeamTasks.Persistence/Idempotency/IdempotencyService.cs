using Microsoft.EntityFrameworkCore;
using TeamTasks.Application.Core.Abstractions;
using TeamTasks.Application.Core.Abstractions.Idempotency;

namespace TeamTasks.Persistence.Idempotency;

/// <summary>
/// Represents the idempotency service class.
/// </summary>
internal sealed class IdempotencyService
    : IIdempotencyService
{
    private readonly BaseDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of <see cref="IdempotencyService"/>.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public IdempotencyService(
        BaseDbContext dbContext,
        IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<bool> RequestExistsAsync(Guid requestId) =>
         await _dbContext
             .Set<IdempotentRequest>()
             .AnyAsync(r => r.Id == requestId);
    

    /// <inheritdoc />
    public async Task CreateRequestAsync(Guid requestId, string name)
    {
        IdempotentRequest idempotentRequest = new IdempotentRequest
        {
            Id = requestId,
            Name = name,
            CreatedOnUtc = DateTime.UtcNow
        };

        await _dbContext
            .Set<IdempotentRequest>()
            .AddAsync(idempotentRequest);
        
        await _dbContext.SaveChangesAsync();
    }
}