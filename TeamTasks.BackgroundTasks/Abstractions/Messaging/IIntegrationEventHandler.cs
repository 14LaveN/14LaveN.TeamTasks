﻿using MediatR;
using TeamTasks.Application.Core.Abstractions.Messaging;

namespace TeamTasks.BackgroundTasks.Abstractions.Messaging;

/// <summary>
/// Represents the integration event handler.
/// </summary>
/// <typeparam name="TIntegrationEvent">The integration event type.</typeparam>
public interface IIntegrationEventHandler<in TIntegrationEvent> : INotificationHandler<TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent;