using TeamTasks.Domain.Core.Utility;

namespace TeamTasks.Identity.Api.Common.DependencyInjection;

internal static class DiConsul
{
    /// <summary>
    /// Registers the necessary services with the DI framework.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddDatabase(this IServiceCollection services,
        IConfiguration configuration)
    {
        Ensure.NotNull(services, "Services is required.", nameof(services));

        
        
        return services;
    }
}