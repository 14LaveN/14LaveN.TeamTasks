using MediatR.NotificationPublishers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using TeamTasks.Application.Core.Abstractions.Common;
using TeamTasks.Application.Core.Abstractions.Events;
using TeamTasks.Application.Core.Abstractions.Helpers.JWT;
using TeamTasks.Domain.Core.Utility;
using TeamTasks.Identity.Infrastructure.Authentication;

namespace TeamTasks.Identity.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the necessary services with the DI framework.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services)
    {
        Ensure.NotNull(services, "Services is required.", nameof(services));
        
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
        
        return services;
    }
}