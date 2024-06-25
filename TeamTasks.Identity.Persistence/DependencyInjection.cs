using Microsoft.Extensions.DependencyInjection;
using TeamTasks.Identity.Domain.Repositories;
using TeamTasks.Identity.Persistence.Repositories;

namespace TeamTasks.Identity.Persistence;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the necessary services with the DI framework.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddUserDatabase(this IServiceCollection services)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        
        services.AddScoped<IUserRepository, UserRepository>();
        
        return services;
    }
}