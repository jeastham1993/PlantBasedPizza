using Microsoft.Extensions.Configuration;
using OpenTelemetry.Resources;

namespace PlantBasedPizza.Shared;

public static class ResourceBuilderExtensions
{
    public static ResourceBuilder AddDefaultOtelTags(this ResourceBuilder resourceBuilder, IConfiguration configuration)
    {
        var defaultApplicationConfig = configuration.GetSection("ApplicationConfig").Get<DefaultApplicationConfig>();
        
        if (defaultApplicationConfig == null) return resourceBuilder;
        
        resourceBuilder.AddAttributes(new List<KeyValuePair<string, object>>()
        {
            new("service.name", defaultApplicationConfig.ApplicationName),
            new("service.environment", defaultApplicationConfig.Environment),
            new("service.team", defaultApplicationConfig.TeamName),
            new("service.version", defaultApplicationConfig.Version),
            new("instance.memory_mb", defaultApplicationConfig.MemoryMb),
            new("instance.cpu_count.", defaultApplicationConfig.CpuCount),
            new("cloud.region.", defaultApplicationConfig.CloudRegion),
            new("service.build.id", defaultApplicationConfig.Version),
            new("service.build.deployment.at", defaultApplicationConfig.DeployedAt)
        });
        
        return resourceBuilder;
    }
}