using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.FSx;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SSM;
using Constructs;

namespace PlantBasedPizza.Infra.Constructs;

public record ConstructProps(IVpc Vpc, ICluster cluster, string ServiceName, string Environment, string DataDogApiKeyParameterName, string JwtKeyParameterName, string RepositoryName, string Tag, double Port, Dictionary<string, string> EnvironmentVariables, Dictionary<string, Amazon.CDK.AWS.ECS.Secret> Secrets, string? SharedLoadBalancerArn, string? SharedListenerArn, string HealthCheckPath, string PathPattern, int Priority, string InternalSharedLoadBalancerArn = null, string InternalSharedListenerArn = null, bool DeployInPrivateSubnet = false);

public class WebService : Construct
{
    public IRole ExecutionRole { get; private set; }
    public IRole TaskRole { get; private set; }
    public IApplicationLoadBalancer LoadBalancer { get; private set; }
    
    public WebService(Construct scope, string id, ConstructProps props) : base(scope, id)
    {
        var ddApiKeyParam = StringParameter.FromSecureStringParameterAttributes(this, "DDApiKey",
            new SecureStringParameterAttributes
            {
                ParameterName = props.DataDogApiKeyParameterName
            });
        var jwtKeyParam = StringParameter.FromSecureStringParameterAttributes(this, "JWTKeyParam",
            new SecureStringParameterAttributes
            {
                ParameterName = props.JwtKeyParameterName
            });
        
        var repository = Repository.FromRepositoryName(this, $"{props.ServiceName}Repo", props.RepositoryName);

        ExecutionRole = new Role(this, $"{props.ServiceName}ExecutionRole", new RoleProps()
        {
            AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com")
        });
        
        ExecutionRole.AddManagedPolicy(ManagedPolicy.FromManagedPolicyArn(this, "TaskExecutionPolicy", "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"));
        
        TaskRole = new Role(this, $"{props.ServiceName}TaskRole", new RoleProps()
        {
            AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com")
        });

        var baseEnvironmentVariables = new Dictionary<string, string>
        {
            { "OtlpUseHttp", "Y" },
            { "Environment", props.Environment },
            { "ServiceDiscovery__MyUrl", "" },
            { "ServiceDiscovery__ServiceName", "" },
            { "ServiceDiscovery__ConsulServiceEndpoint", "" },
            { "Auth__Issuer", "https://plantbasedpizza.com" },
            { "Auth__Audience", "https://plantbasedpizza.com" },
            { "ECS_ENABLE_CONTAINER_METADATA", "true" },
            { "ENV", props.Environment},
            { "DD_ENV", props.Environment},
            { "DD_SERVICE", props.ServiceName},
            { "DD_VERSION", props.Tag},
            { "DD_GIT_COMMIT_SHA", props.Tag},
            { "DD_GIT_REPOSITORY_URL", "https://github.com/jeastham1993/PlantBasedPizza"},
            { "DD_AGENT_HOST", "127.0.0.1"},
            { "DD_TRACE_ROUTE_TEMPLATE_RESOURCE_NAMES_ENABLED", "true"},
            { "DD_RUNTIME_METRICS_ENABLED", "true"},
        };
        var baseSecrets = new Dictionary<string, Secret>()
        {
            { "Auth__Key", Secret.FromSsmParameter(jwtKeyParam) }
        };

        var taskDefinition = new FargateTaskDefinition(this, $"{props.ServiceName}Definition", new FargateTaskDefinitionProps
        {
            MemoryLimitMiB = 512,
            RuntimePlatform = new RuntimePlatform
            {
                CpuArchitecture = CpuArchitecture.X86_64,
                OperatingSystemFamily = OperatingSystemFamily.LINUX
            },
            ExecutionRole = ExecutionRole,
            TaskRole = TaskRole,
        });
        var container = taskDefinition.AddContainer("application", new ContainerDefinitionOptions()
        {
            Image = ContainerImage.FromEcrRepository(repository, props.Tag ?? "latest"),
            PortMappings = new []{new PortMapping()
            {
                ContainerPort = props.Port,
                Protocol = Amazon.CDK.AWS.ECS.Protocol.TCP
            }},
            ContainerName = props.ServiceName,
            Environment = baseEnvironmentVariables.Union(props.EnvironmentVariables).ToDictionary(x => x.Key, x => x.Value),
            Secrets = baseSecrets.Union(props.Secrets).ToDictionary(x => x.Key, x => x.Value),
            Logging = LogDrivers.Firelens(new FireLensLogDriverProps
            {
                Options = new Dictionary<string, string>
                {
                    { "Name", "datadog" },
                    { "Host", "http-intake.logs.datadoghq.eu" },
                    { "TLS", "on" },
                    { "dd_service", props.ServiceName },
                    { "dd_source", "aspnet" },
                    { "dd_message_key", "log" },
                    { "dd_tags", $"project:{props.ServiceName}" },
                    { "provider", "ecs" }
                },
                SecretOptions = new Dictionary<string, Secret>(1)
                {
                    { "apikey", Secret.FromSsmParameter(ddApiKeyParam) },
                }
            }),
        });
        container.AddDockerLabel("com.datadoghq.tags.env", props.Environment);
        container.AddDockerLabel("com.datadoghq.tags.service", props.ServiceName);
        container.AddDockerLabel("com.datadoghq.tags.version", props.Tag);

        taskDefinition.AddContainer("datadog-agent", new ContainerDefinitionOptions()
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
                     { "DD_APM_ENABLED", "true"},
                     { "DD_APM_NON_LOCAL_TRAFFIC", "true"},
                     { "DD_DOGSTATSD_NON_LOCAL_TRAFFIC", "true"},
                     { "DD_ENV", props.Environment},
                     { "DD_SERVICE", props.ServiceName},
                     { "DD_VERSION", props.Tag},
                     { "DD_APM_IGNORE_RESOURCES", $"(GET) {props.HealthCheckPath}"}
                 },
                 Secrets = new Dictionary<string, Secret>(1)
                 {
                     { "DD_API_KEY", Secret.FromSsmParameter(ddApiKeyParam) }
                 }
        });

        taskDefinition.AddFirelensLogRouter("firelens", new FirelensLogRouterDefinitionOptions
        {
            Essential = true,
            Image = ContainerImage.FromRegistry("amazon/aws-for-fluent-bit:stable"),
            ContainerName = "log-router",
            FirelensConfig = new FirelensConfig
            {
                Type = FirelensLogRouterType.FLUENTBIT,
                Options = new FirelensOptions
                {
                    EnableECSLogMetadata = true
                }
            },
        });

        var service = new FargateService(this, $"{props.ServiceName}Service", new FargateServiceProps()
        {
            Cluster = props.cluster,
            TaskDefinition = taskDefinition,
            DesiredCount = 1,
            AssignPublicIp = !props.DeployInPrivateSubnet,
            VpcSubnets = new SubnetSelection()
            {
                Subnets = props.DeployInPrivateSubnet ? props.Vpc.PrivateSubnets : props.Vpc.PublicSubnets
            }
        });
        
        ddApiKeyParam.GrantRead(ExecutionRole);
        jwtKeyParam.GrantRead(ExecutionRole);
        repository.GrantPull(ExecutionRole);
        
        if (!string.IsNullOrEmpty(props.SharedListenerArn) && !string.IsNullOrEmpty(props.SharedLoadBalancerArn))
        {
            var targetGroup = new ApplicationTargetGroup(this, $"{props.ServiceName}TargetGroup", new ApplicationTargetGroupProps()
            {
                Port = 8080,
                Targets = new List<IApplicationLoadBalancerTarget>(1)
                {
                    service
                }.ToArray(),
                HealthCheck = new Amazon.CDK.AWS.ElasticLoadBalancingV2.HealthCheck()
                {
                    Port = props.Port.ToString(),
                    Path = props.HealthCheckPath,
                    HealthyHttpCodes = "200-404",
                },
                Vpc = props.Vpc
            });
            
            var sharedListener = ApplicationListener.FromLookup(this, "SharedHttpListener",
                new ApplicationListenerLookupOptions()
                {
                    LoadBalancerArn =
                        props.SharedLoadBalancerArn,
                    ListenerPort = 80,
                    ListenerArn = props.SharedListenerArn
                });
            
            sharedListener.AddTargetGroups("ECS", new AddApplicationTargetGroupsProps()
            {
                Conditions = [ListenerCondition.PathPatterns([props.PathPattern])],
                Priority = props.Priority,
                TargetGroups = [targetGroup]
            });
        }

        if (!string.IsNullOrEmpty(props.InternalSharedListenerArn) && !string.IsNullOrEmpty(props.InternalSharedLoadBalancerArn))
        {
            var internalTargetGroup = new ApplicationTargetGroup(this, $"{props.ServiceName}InternalTargetGroup", new ApplicationTargetGroupProps()
            {
                Port = 8080,
                Targets = new List<IApplicationLoadBalancerTarget>(1)
                {
                    service
                }.ToArray(),
                HealthCheck = new Amazon.CDK.AWS.ElasticLoadBalancingV2.HealthCheck()
                {
                    Port = props.Port.ToString(),
                    Path = props.HealthCheckPath,
                    HealthyHttpCodes = "200-404"
                },
                Vpc = props.Vpc
            });
            
            var internalListener = ApplicationListener.FromLookup(this, "InternalHttpListener",
                new ApplicationListenerLookupOptions()
                {
                    LoadBalancerArn =
                        props.InternalSharedLoadBalancerArn,
                    ListenerPort = 80,
                    ListenerArn = props.InternalSharedListenerArn
                });
            internalListener.AddTargetGroups("ECS", new AddApplicationTargetGroupsProps()
            {
                Conditions = [ListenerCondition.PathPatterns([props.PathPattern])],
                Priority = props.Priority,
                TargetGroups = [internalTargetGroup]
            });
        }
    }
}