using MediatR.NotificationPublishers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using TeamTasks.Application.Core.Behaviours;
using TeamTasks.Domain.Core.Utility;
using TeamTasks.Identity.Application.Behaviour;
using TeamTasks.Identity.Application.Commands;
using TeamTasks.Identity.Application.Queries.GetTheUserById;

namespace TeamTasks.Identity.Application;

public static class DiMediator
{
    /// <summary>
    /// Registers the necessary services with the DI framework.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddMediatr(this IServiceCollection services)
    {
        Ensure.NotNull(services, "Services is required.", nameof(services));

        services.AddMediatR(x =>
        {
            x.RegisterServicesFromAssemblyContaining<Program>();

            x.RegisterServicesFromAssemblies(typeof(Register.Command).Assembly,
                    typeof(Register.CommandHandler).Assembly)
                .RegisterServicesFromAssemblies(typeof(Login.Command).Assembly,
                    typeof(Login.CommandHandler).Assembly)
                .RegisterServicesFromAssemblies(typeof(ChangePassword.Command).Assembly,
                    typeof(ChangePassword.CommandHandler).Assembly)
                .RegisterServicesFromAssemblies(typeof(ChangeName.Command).Assembly,
                    typeof(ChangeName.CommandHandler).Assembly)
                .RegisterServicesFromAssemblies(typeof(GetTheUserByIdQuery).Assembly,
                    typeof(GetTheUserByIdQueryHandler).Assembly);
            
            x.AddOpenBehavior(typeof(QueryCachingBehavior<,>))
                //TODO .AddOpenBehavior(typeof(IdentityIdempotentCommandPipelineBehavior<,>))
                //TODO .AddOpenBehavior(typeof(UserTransactionBehavior<,>))
                .AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>))
                .AddOpenBehavior(typeof(ValidationBehaviour<,>))
                .AddOpenBehavior(typeof(MetricsBehaviour<,>));
            
            x.NotificationPublisher = new TaskWhenAllPublisher();
            x.NotificationPublisherType = typeof(TaskWhenAllPublisher);
        });
        
        return services;
    }
}