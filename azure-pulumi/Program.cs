using Pulumi.AzureNative.Resources;
using System.Collections.Generic;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;

return await Pulumi.Deployment.RunAsync(() =>
{
    var config = new Pulumi.Config();
    var ddApiKey = config.RequireSecret("DD_API_KEY");
    var dbConnectionString = config.RequireSecret("DB_CONNECTION_STRING");
    var imageTag = config.Require("IMAGE_TAG");
    
    // Create an Azure Resource Group
    var resourceGroup = new ResourceGroup("PlantBasedPizza-Pulumi");

    var environment = new ManagedEnvironment("managedEnv", new ManagedEnvironmentArgs
    {
        Location = resourceGroup.Location,
        ResourceGroupName = resourceGroup.Name,
        EnvironmentName = "dev-pulumi",
        Sku = new EnvironmentSkuPropertiesArgs
        {
            Name = SkuName.Consumption
        }
    });

    var containerApp = new ContainerApp("monolith", new()
    {
        ContainerAppName = "monolith",
        EnvironmentId = environment.Id,
        ResourceGroupName = resourceGroup.Name,
        Location = resourceGroup.Location,
        Template = new TemplateArgs()
        {
            Containers = new List<ContainerArgs>()
            {
                new()
                {
                    Name = "application",
                    Image =
                        $"plantpowerjames/plant-based-pizza-monolith:{imageTag}",
                    Resources = new ContainerResourcesArgs()
                    {
                        Cpu = 0.25,
                        Memory = "0.5Gi"
                    },
                    Env = new List<EnvironmentVarArgs>()
                    {
                        new() { Name = "DatabaseConnection", SecretRef = "database-connection" },
                        new() { Name = "Environment", Value = "dev" },
                        new() { Name = "OtlpEndpoint", Value = "http://localhost:4317" }
                    }
                },
                new ()
                {
                    Name = "datadog-agent",
                    Image = "index.docker.io/datadog/serverless-init:latest",
                    Resources = new ContainerResourcesArgs()
                    {
                        Cpu = 0.25,
                        Memory = "0.5Gi"
                    },
                    Env = new List<EnvironmentVarArgs>()
                    {
                        new() { Name = "DD_SITE", Value = "datadoghq.eu"},
                        new() { Name = "DD_API_KEY", SecretRef = "dd-api-key" },
                        new() { Name = "DD_ENV", Value = "dev" },
                        new() { Name = "DD_VERSION", Value = imageTag },
                        new() { Name = "DD_SERVICE", Value = "monolith" },
                        new() { Name = "DD_LOGS_ENABLED", Value = "true" },
                        new() { Name = "DD_LOGS_INJECTION", Value = "true" },
                        new() { Name = "DD_APM_IGNORE_RESOURCES", Value = "/opentelemetry.proto.collector.trace.v1.TraceService/Export$" },
                        new() { Name = "DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_GRPC_ENDPOINT", Value = "0.0.0.0:4317" },
                        new() { Name = "DD_AZURE_SUBSCRIPTION_ID", Value = "6d2b072c-7905-4816-b79c-c69bbc5099f3" },
                        new() { Name = "DD_AZURE_RESOURCE_GROUP", Value = resourceGroup.Name },
                        
                    }
                }
            }
        },
        Configuration = new ConfigurationArgs()
        {
            Secrets = new List<SecretArgs>()
            {
                new() { Name = "dd-api-key", Value = ddApiKey },
                new()
                {
                    Name = "database-connection", Value = dbConnectionString
                }
            },
            Ingress = new IngressArgs()
            {
                External = true,
                TargetPort = 8080,
                Traffic = new TrafficWeightArgs()
                {
                    Weight = 100,
                    LatestRevision = true
                }
            }
        }
    });
});