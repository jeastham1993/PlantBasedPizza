using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace PlantBasedPizza.Shared.Caching;

public static class MomentoCacheServiceExtensions
{
    public static IServiceCollection AddMomentoCache(this IServiceCollection services,
        Action<MomentoCacheConfiguration> configuration)
    {
        services.AddOptions();

        services.Configure(configuration);
        services.Add(ServiceDescriptor.Singleton<IDistributedCache, MomentoDistributedCache>());

        return services;
    }
}