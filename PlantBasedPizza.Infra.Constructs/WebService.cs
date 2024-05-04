using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SSM;
using Constructs;

namespace PlantBasedPizza.Infra.Constructs;

public record ConstructProps(IVpc Vpc, ICluster cluster, string ServiceName, string DataDogApiKeyParameterName, string JwtKeyParameterName, string RepositoryName, string Tag, double Port, Dictionary<string, string> EnvironmentVariables, Dictionary<string, Amazon.CDK.AWS.ECS.Secret> Secrets, string SharedLoadBalancerArn, string SharedListenerArn, string HealthCheckPath, string PathPattern, int Priority);

public class WebService : Construct
{
    public IRole ExecutionRole { get; private set; }
    public IRole TaskRole { get; private set; }
    
    public WebService(Construct scope, string id, ConstructProps props) : base(scope, id)
    {
        var sharedListener = ApplicationListener.FromLookup(this, "SharedHttpListener",
            new ApplicationListenerLookupOptions()
            {
                LoadBalancerArn =
                    props.SharedLoadBalancerArn,
                ListenerPort = 80,
                ListenerArn = props.SharedListenerArn
            });
        
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
            { "OtlpEndpoint", "http://127.0.0.1:4318/v1/traces" },
            { "OtlpUseHttp", "Y" },
            { "Environment", "dev" },
            { "ServiceDiscovery__MyUrl", "" },
            { "ServiceDiscovery__ServiceName", "" },
            { "ServiceDiscovery__ConsulServiceEndpoint", "" },
            { "Auth__Issuer", "https://plantbasedpizza.com" },
            { "Auth__Audience", "https://plantbasedpizza.com" },
            { "ECS_ENABLE_CONTAINER_METADATA", "true" },
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
        taskDefinition.AddContainer("application", new ContainerDefinitionOptions()
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
                    { "dd_service", props.ServiceName },
                    { "dd_source", "aspnet" },
                    { "dd_message_key", "log" },
                    { "dd_tags", "project:fluentbit" },
                    { "TLS", "on" },
                    { "provider", "ecs" }
                },
                SecretOptions = new Dictionary<string, Secret>(1)
                {
                    { "apikey", Secret.FromSsmParameter(ddApiKeyParam) },
                }
            }),
        });
        taskDefinition.AddContainer("datadog-agent", new ContainerDefinitionOptions()
        {
            Image = ContainerImage.FromRegistry("public.ecr.aws/datadog/agent:latest"),
                 PortMappings = new []
                 {
                     new PortMapping()
                     {
                         ContainerPort = 4318
                     }
                 },
                 ContainerName = "datadog-agent",
                 Environment = new Dictionary<string, string>
                 {
                     { "DD_SITE", "datadoghq.eu" },
                     { "ECS_FARGATE", "true" },
                     { "DD_LOGS_ENABLED", "true" },
                     { "DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_HTTP_ENDPOINT", "0.0.0.0:4318" }
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
            }
        });

        var service = new FargateService(this, $"{props.ServiceName}Service", new FargateServiceProps()
        {
            Cluster = props.cluster,
            TaskDefinition = taskDefinition,
            DesiredCount = 1,
            AssignPublicIp = true
        });

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
                HealthyHttpCodes = "200-404"
            },
            Vpc = props.Vpc
        });

        sharedListener.AddTargetGroups("ECS", new AddApplicationTargetGroupsProps()
        {
            Conditions = new[]{ListenerCondition.PathPatterns(new []{props.PathPattern})},
            Priority = props.Priority,
            TargetGroups = new IApplicationTargetGroup[1]{targetGroup}
        });
        
        ddApiKeyParam.GrantRead(ExecutionRole);
        jwtKeyParam.GrantRead(ExecutionRole);
        repository.GrantPull(ExecutionRole);
    }
}