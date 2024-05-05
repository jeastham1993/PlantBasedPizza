using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.SSM;
using Constructs;
using HealthCheck = Amazon.CDK.AWS.ECS.HealthCheck;
using Protocol = Amazon.CDK.AWS.ElasticLoadBalancingV2.Protocol;

namespace Infra
{
    public class RecipeApiInfraStack : Stack
    {
        internal RecipeApiInfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var bus = EventBus.FromEventBusName(this, "SharedEventBus", "PlantBasedPizzaEvents");

            var vpc = Vpc.FromLookup(this, "MainVpc", new VpcLookupOptions()
            {
                VpcId = "vpc-06c60c0d760921bc6",
            });

            var databaseConnectionParam = StringParameter.FromSecureStringParameterAttributes(this, "DatabaseParameter",
                new SecureStringParameterAttributes()
                {
                    ParameterName = "/shared/database-connection"
                });
            var ddApiKeyParam = StringParameter.FromSecureStringParameterAttributes(this, "DDApiKey",
                new SecureStringParameterAttributes()
                {
                    ParameterName = "/shared/dd-api-key"
                });
            var jwtKeyParam = StringParameter.FromSecureStringParameterAttributes(this, "JWTKeyParam",
                new SecureStringParameterAttributes()
                {
                    ParameterName = "/shared/jwt-key"
                });

            var cluster = new Cluster(this, "RecipeServiceCluster", new ClusterProps(){
                Vpc = vpc
            });

            var repository = Repository.FromRepositoryName(this, "RecipeApiRepo", "recipe-api");
            
            var ecsService =
                new ApplicationLoadBalancedFargateService(this, "ApiService",
                    new ApplicationLoadBalancedFargateServiceProps(){
                        Cluster = cluster,
                        MemoryLimitMiB = 512,
                        DesiredCount = 1,
                        TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions()
                        {
                            Image = ContainerImage.FromEcrRepository(repository),
                            ContainerPort = 8080,
                            ContainerName = "recipe-api",
                            Environment = new Dictionary<string, string>()
                            {
                                {"OtlpEndpoint", "http://0.0.0.0:4317"},
                                {"Environment", "dev"},
                                {"ServiceDiscovery__MyUrl", ""},
                                {"ServiceDiscovery__ServiceName", ""},
                                {"ServiceDiscovery__ConsulServiceEndpoint", ""},
                                {"Messaging__BusName", bus.EventBusName},
                                {"Auth__Issuer", "https://plantbasedpizza.com"},
                                {"Auth__Audience", "https://plantbasedpizza.com"},
                            },
                            Secrets = new Dictionary<string, Secret>(1)
                            {
                                {"DatabaseConnection", Secret.FromSsmParameter(databaseConnectionParam)},
                                {"Auth__Key", Secret.FromSsmParameter(jwtKeyParam)}
                            }
                        },
                        ServiceName = "recipe-service",
                        Protocol = ApplicationProtocol.HTTP,
                        PublicLoadBalancer = true,
                        LoadBalancerName = "recipe-service-alb",
                        RuntimePlatform = new RuntimePlatform()
                        {
                            CpuArchitecture = CpuArchitecture.X86_64,
                            OperatingSystemFamily = OperatingSystemFamily.LINUX
                        },
                        AssignPublicIp = true,
                    });
            
            ecsService.Service.EnableServiceConnect(new ServiceConnectProps()
            {
                Namespace = "plantbasedpizza"
            });
            
            ecsService.TargetGroup.ConfigureHealthCheck(new Amazon.CDK.AWS.ElasticLoadBalancingV2.HealthCheck()
            {
                Enabled = true,
                HealthyHttpCodes = "200-404",
                Path = "/recipes/health",
                Port = "8080",
                Protocol = Protocol.HTTP,
                UnhealthyThresholdCount = 5
            });

            var container = ecsService.TaskDefinition.AddContainer("datadog-agent", new ContainerDefinitionOptions()
            {
                Image = ContainerImage.FromRegistry("public.ecr.aws/datadog/agent:latest"),
                PortMappings = new List<IPortMapping>(1){
                    new PortMapping
                    {
                        ContainerPort = 4317,
                        HostPort = 4317,
                    }
                }.ToArray(),
                ContainerName = "dd-agent",
                Cpu = 256,
                Environment = new Dictionary<string, string>()
                {
                    { "DD_SITE", "datadoghq.eu" },
                    { "ECS_FARGATE", "true" },
                    { "DD_LOGS_ENABLED", "true"},
                    { "DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_GRPC_ENDPOINT", "0.0.0.0:4317" }
                },
                Secrets = new Dictionary<string, Secret>(1)
                {
                    {"DD_API_KEY", Secret.FromSsmParameter(ddApiKeyParam)}
                },
                MemoryLimitMiB = 512,
                HealthCheck = new HealthCheck()
                {
                    Retries = 3,
                    Command = new []
                    {
                        "CMD-SHELL",
                        "agent health"
                    },
                    Timeout = Duration.Seconds(5),
                    Interval = Duration.Seconds(30),
                    StartPeriod = Duration.Seconds(30)
                }
            });

            ddApiKeyParam.GrantRead(ecsService.TaskDefinition.ExecutionRole);
            databaseConnectionParam.GrantRead(ecsService.TaskDefinition.ExecutionRole);
            repository.GrantPull(ecsService.TaskDefinition.ExecutionRole);
            bus.GrantPutEventsTo(ecsService.TaskDefinition.TaskRole);
        }
    }
}
