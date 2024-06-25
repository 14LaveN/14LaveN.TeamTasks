using TeamTasks.Domain.Core.Utility;

namespace TeamTasks.Identity.Api.Common.DependencyInjection;

internal static class DiCaching
{
    /// <summary>
    /// Registers the necessary services with the DI framework.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        Ensure.NotNull(services, "Services is required.", nameof(services));

        services.AddResponseCaching(options =>
        {
            options.UseCaseSensitivePaths = false; 
            options.MaximumBodySize = 1024; 
        });
        
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "Identity_";
        });

        services.AddDistributedMemoryCache();
        
        return services;
    }
}