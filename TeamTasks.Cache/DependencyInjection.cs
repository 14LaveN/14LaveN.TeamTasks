using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeamTasks.Domain.Core.Utility;

namespace TeamTasks.Cache;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the necessary services with the DI framework.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddCache(
        this IServiceCollection services)
    {
        Ensure.NotNull(services, "Services is required.", nameof(services));
        
        services.AddResponseCaching(options =>
        {
            options.UseCaseSensitivePaths = false; 
            options.MaximumBodySize = 1024; 
        });

        services.AddMemoryCache(options =>
        {
            options.TrackLinkedCacheEntries = true;
            options.TrackStatistics = true;
        });

        services.AddDistributedMemoryCache(options =>
        {
            options.TrackStatistics = true;
            options.TrackLinkedCacheEntries = true;
        });
        
        return services;
    }
}