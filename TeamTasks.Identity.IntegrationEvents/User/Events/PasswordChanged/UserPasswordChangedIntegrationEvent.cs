﻿using Newtonsoft.Json;
using TeamTasks.Application.Core.Abstractions.Messaging;
using TeamTasks.Identity.Domain.Events.User;

namespace TeamTasks.Identity.IntegrationEvents.User.Events.PasswordChanged;

/// <summary>
/// Represents the integration event that is raised when a user's password is changed.
/// </summary>
public sealed class UserPasswordChangedIntegrationEvent : IIntegrationEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserPasswordChangedIntegrationEvent"/> class.
    /// </summary>
    /// <param name="userPasswordChangedDomainEvent">The user password changed domain event.</param>
    public UserPasswordChangedIntegrationEvent(UserPasswordChangedDomainEvent userPasswordChangedDomainEvent) =>
        Id = userPasswordChangedDomainEvent.User.Id;

    [JsonConstructor]
    private UserPasswordChangedIntegrationEvent(Guid userId) => Id = userId;

    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public Guid Id { get; }
}