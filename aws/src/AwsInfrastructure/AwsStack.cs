using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Constructs;
using HealthCheck = Amazon.CDK.AWS.ElasticLoadBalancingV2.HealthCheck;

namespace AwsInfrastructure;

public class AwsStack : Stack
{
    internal AwsStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        var vpc = new Vpc(this, "PlantBasedPizzaNetwork", new VpcProps
        {
            MaxAzs = 2
        });

        var cluster = new Cluster(this, "PlantBasedPizzaCluster", new ClusterProps
        {
            Vpc = vpc
        });

        // Create a load-balanced Fargate service and make it public
        var fargateService = new ApplicationLoadBalancedFargateService(this, "PlantBasedPizzaMonolith",
            new ApplicationLoadBalancedFargateServiceProps
            {
                Cluster = cluster,
                DesiredCount = 1,
                RuntimePlatform = new RuntimePlatform
                {
                    CpuArchitecture = CpuArchitecture.X86_64,
                    OperatingSystemFamily = OperatingSystemFamily.LINUX
                },
                TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                {
                    Image = ContainerImage.FromRegistry("plantpowerjames/plant-based-pizza-monolith:18098b9"),
                    Environment = new Dictionary<string, string>()
                    {
                        { "Environment", "dev" },
                        {
                            "DatabaseConnection",
                            System.Environment.GetEnvironmentVariable("DATABASE_CONNECTION")
                        }
                    },
                    ContainerPort = 8080,
                    ContainerName = "web"
                },
                MemoryLimitMiB = 512,
                PublicLoadBalancer = true
            }
        );

        fargateService.TargetGroup.HealthCheck = new HealthCheck()
        {
            Port = "8080",
            Path = "/",
            HealthyHttpCodes = "200-404"
        };

        fargateService.TaskDefinition.AddContainer("datadog-agent", new ContainerDefinitionOptions()
        {
            Image = ContainerImage.FromRegistry("public.ecr.aws/datadog/agent:latest"),
            PortMappings =
            [
                new PortMapping()
                {
                    ContainerPort = 4317
                },
                new PortMapping()
                {
                    ContainerPort = 4318
                },
                new PortMapping()
                {
                    ContainerPort = 8126
                }
            ],
            ContainerName = "datadog-agent",
            Environment = new Dictionary<string, string>
            {
                { "DD_SITE", "datadoghq.eu" },
                { "ECS_FARGATE", "true" },
                { "DD_LOGS_ENABLED", "false" },
                { "DD_PROCESS_AGENT_ENABLED", "true" },
                { "DD_APM_ENABLED", "true" },
                { "DD_APM_NON_LOCAL_TRAFFIC", "true" },
                { "DD_DOGSTATSD_NON_LOCAL_TRAFFIC", "true" },
                { "DD_ENV", "dev" },
                { "DD_SERVICE", "plantbasedpizza" },
                { "DD_VERSION", "18098b9" },
                { "DD_API_KEY", System.Environment.GetEnvironmentVariable("DD_API_KEY") },
            },
            // Secrets = new Dictionary<string, Secret>(1)
            // {
            //     { "DD_API_KEY", Secret.FromSsmParameter(ddApiKeyParam) }
            // }
        });
    }
}