using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Momento.Sdk.Auth;
using Momento.Sdk.Config;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace PlantBasedPizza.Shared.Caching;

public static class Setup
{
    public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        var momentoApiKey = configuration.GetValue<string>("MOMENTO_API_KEY");
        var cacheName = configuration.GetValue<string>("CACHE_NAME");

        if (string.IsNullOrEmpty(momentoApiKey) || string.IsNullOrEmpty(cacheName))
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddMomentoCache(options =>
            {
                options.Configuration = Configurations.InRegion.LowLatency.Latest();
                options.CredentialProvider = new StringMomentoTokenProvider(momentoApiKey);
                options.DefaultTtl = TimeSpan.FromMinutes(1);
                options.CacheName = cacheName;
            });
        }

        return services;
    }
}