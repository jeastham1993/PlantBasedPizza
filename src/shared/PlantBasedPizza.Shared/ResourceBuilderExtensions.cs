using Microsoft.Extensions.Configuration;
using OpenTelemetry.Resources;

namespace PlantBasedPizza.Shared;

public static class ResourceBuilderExtensions
{
    public static ResourceBuilder AddDefaultOtelTags(this ResourceBuilder resourceBuilder, IConfiguration configuration)
    {
        var defaultApplicationConfig = configuration.GetSection("ApplicationConfig").Get<DefaultApplicationConfig>();

        if (defaultApplicationConfig == null) return resourceBuilder;

        var attributes = new List<KeyValuePair<string, object>>();

        if (!string.IsNullOrEmpty(defaultApplicationConfig.ApplicationName))
            attributes.Add(new KeyValuePair<string, object>("service.name", defaultApplicationConfig.ApplicationName));

        if (!string.IsNullOrEmpty(defaultApplicationConfig.Environment))
            attributes.Add(
                new KeyValuePair<string, object>("service.environment", defaultApplicationConfig.Environment));

        if (!string.IsNullOrEmpty(defaultApplicationConfig.TeamName))
            attributes.Add(new KeyValuePair<string, object>("service.team", defaultApplicationConfig.TeamName));

        if (!string.IsNullOrEmpty(defaultApplicationConfig.Version))
        {
            attributes.Add(new KeyValuePair<string, object>("service.version", defaultApplicationConfig.Version));
            attributes.Add(new KeyValuePair<string, object>("service.build.id", defaultApplicationConfig.Version));
        }

        if (!string.IsNullOrEmpty(defaultApplicationConfig.MemoryMb))
            attributes.Add(new KeyValuePair<string, object>("container.memory_mb", defaultApplicationConfig.MemoryMb));

        if (!string.IsNullOrEmpty(defaultApplicationConfig.CpuCount))
            attributes.Add(new KeyValuePair<string, object>("container.cpu_count", defaultApplicationConfig.CpuCount));

        if (!string.IsNullOrEmpty(defaultApplicationConfig.CloudRegion))
            attributes.Add(new KeyValuePair<string, object>("cloud.region", defaultApplicationConfig.CloudRegion));

        if (!string.IsNullOrEmpty(defaultApplicationConfig.DeployedAt))
            attributes.Add(new KeyValuePair<string, object>("service.build.deployment.at",
                defaultApplicationConfig.DeployedAt));

        resourceBuilder.AddAttributes(attributes);

        return resourceBuilder;
    }
}