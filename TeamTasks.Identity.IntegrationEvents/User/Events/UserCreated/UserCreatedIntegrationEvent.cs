using Newtonsoft.Json;
using TeamTasks.Application.Core.Abstractions.Messaging;
using TeamTasks.Identity.Domain.Events.User;

namespace TeamTasks.Identity.IntegrationEvents.User.Events.UserCreated;

/// <summary>
/// Represents the integration event that is raised when a user is created.
/// </summary>
public sealed class UserCreatedIntegrationEvent : IIntegrationEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserCreatedIntegrationEvent"/> class.
    /// </summary>
    /// <param name="userCreatedDomainEvent">The user created domain event.</param>
    public UserCreatedIntegrationEvent(UserCreatedDomainEvent userCreatedDomainEvent) => 
        Id = userCreatedDomainEvent.User.Id;
        
    [JsonConstructor]
    public UserCreatedIntegrationEvent(Guid userId) => Id = userId;

    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public Guid Id { get; }
}