using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamTasks.Application.Core.Abstractions;
using TeamTasks.Persistence;
using TeamTasks.Application.Core.Abstractions.Messaging;
using TeamTasks.Identity.Domain.Entities;
using TeamTasks.Persistence;

namespace TeamTasks.Identity.Application.Behaviour;

/// <summary>
/// Represents the <see cref="User"/> transaction behaviour class.
/// </summary>
internal sealed class UserTransactionBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
    where TResponse : class
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly BaseDbContext _userDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserTransactionBehavior{TRequest,TResponse}"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="userDbContext">The base database context.</param>
    public UserTransactionBehavior(
        IUnitOfWork unitOfWork,
        BaseDbContext userDbContext)
    {
        _unitOfWork = unitOfWork;
        _userDbContext = userDbContext;
    }

    /// <inheritdoc/>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IQuery<TResponse>)
        {
            return await next();
        }
        
        var strategy = _userDbContext.EfDatabase.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                TResponse response = await next();

                await transaction.CommitAsync(cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return response;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);

                throw;
            }
        });

        throw new ArgumentException();
    }
}