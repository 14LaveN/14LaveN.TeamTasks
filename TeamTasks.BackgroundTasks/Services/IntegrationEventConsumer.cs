﻿using MediatR;
using TeamTasks.Application.Core.Abstractions.Messaging;

namespace TeamTasks.BackgroundTasks.Services;

/// <summary>
/// Represents the integration event consumer.
/// </summary>
internal sealed class IntegrationEventConsumer : IIntegrationEventConsumer
{
    private readonly IPublisher _publisher;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationEventConsumer"/> class.
    /// </summary>
    /// <param name="publisher">The publisher.</param>
    public IntegrationEventConsumer(IPublisher publisher) =>
        _publisher = publisher;

    /// <inheritdoc />
    public async System.Threading.Tasks.Task Consume(IIntegrationEvent? integrationEvent) =>
        await _publisher.Publish(integrationEvent!);
}