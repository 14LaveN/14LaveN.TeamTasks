using MediatR.NotificationPublishers;
using TeamTasks.Identity.Domain.Events.User;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using TeamTasks.RabbitMq.Messaging;
using TeamTasks.RabbitMq.Messaging.Settings;
using TeamTasks.RabbitMq.Messaging.User.Publishers.UserCreated;

namespace TeamTasks.RabbitMq;

public static class DependencyInjection
{
    public static IServiceCollection AddRabbitMq(this IServiceCollection services,
        IConfiguration configuration)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddMediatR(x =>
        {
            x.RegisterServicesFromAssemblyContaining<Program>();
            
            x.NotificationPublisher = new TaskWhenAllPublisher();
            x.NotificationPublisherType = typeof(TaskWhenAllPublisher);
        });
        
        services.Configure<MessageBrokerSettings>(configuration.GetSection(MessageBrokerSettings.SettingsKey));
        
        services.AddOptions<MessageBrokerSettings>()
            .BindConfiguration(MessageBrokerSettings.SettingsKey)
            .ValidateOnStart();
        
        services.AddScoped<IIntegrationEventPublisher, IntegrationEventPublisher>();

        services.AddHealthChecks()
            .AddRabbitMQ(new Uri(MessageBrokerSettings.AmqpLink));
        
        return services; 
    }
}