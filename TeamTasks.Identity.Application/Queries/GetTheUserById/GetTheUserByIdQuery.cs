using TeamTasks.Application.Core.Abstractions.Messaging;
using TeamTasks.Domain.Common.Core.Primitives.Maybe;
using TeamTasks.Identity.Contracts.Get;

namespace TeamTasks.Identity.Application.Queries.GetTheUserById;

/// <summary>
/// Represents the get user by id query record.
/// </summary>
/// <param name="UserId">The user identifier.</param>
public sealed record GetTheUserByIdQuery(Guid UserId)
    : ICachedQuery<Maybe<GetUserResponse>>
{
    public string Key { get; } = $"get-user-by-{UserId}";
    
    public TimeSpan? Expiration { get; } = TimeSpan.FromMinutes(6);
}