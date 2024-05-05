using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SSM;
using Constructs;
using PlantBasedPizza.Infra.Constructs;
using ApplicationListenerProps = Amazon.CDK.AWS.ElasticLoadBalancingV2.ApplicationListenerProps;
using HealthCheck = Amazon.CDK.AWS.ElasticLoadBalancingV2.HealthCheck;
using Protocol = Amazon.CDK.AWS.ECS.Protocol;

namespace Infra;

public class AccountApiInfraStack : Stack
{
    internal AccountApiInfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        var bus = EventBus.FromEventBusName(this, "SharedEventBus", "PlantBasedPizzaEvents");

        var vpc = Vpc.FromLookup(this, "MainVpc", new VpcLookupOptions
        {
            VpcId = "vpc-06c60c0d760921bc6"
        });

        var sharedAlb = ApplicationLoadBalancer.FromLookup(this, "SharedAlb", new ApplicationLoadBalancerLookupOptions
        {
            LoadBalancerArn =
                "arn:aws:elasticloadbalancing:eu-west-1:730335273443:loadbalancer/app/plant-based-pizza-shared-ingress/1c948325c1df4e86"
        });

        var sharedListener = ApplicationListener.FromLookup(this, "SharedHttpListener",
            new ApplicationListenerLookupOptions
            {
                LoadBalancerArn =
                    "arn:aws:elasticloadbalancing:eu-west-1:730335273443:loadbalancer/app/plant-based-pizza-shared-ingress/1c948325c1df4e86",
                ListenerPort = 80,
                ListenerArn =
                    "arn:aws:elasticloadbalancing:eu-west-1:730335273443:listener/app/plant-based-pizza-ingress/d99d1b57574af81c/396097df348029f2"
            });

        var databaseConnectionParam = StringParameter.FromSecureStringParameterAttributes(this, "DatabaseParameter",
            new SecureStringParameterAttributes
            {
                ParameterName = "/shared/database-connection"
            });
        var ddApiKeyParam = StringParameter.FromSecureStringParameterAttributes(this, "DDApiKey",
            new SecureStringParameterAttributes
            {
                ParameterName = "/shared/dd-api-key"
            });
        var jwtKeyParam = StringParameter.FromSecureStringParameterAttributes(this, "JWTKeyParam",
            new SecureStringParameterAttributes
            {
                ParameterName = "/shared/jwt-key"
            });

        var cluster = new Cluster(this, "AccountServiceCluster", new ClusterProps
        {
            Vpc = vpc
        });

        var repository = Repository.FromRepositoryName(this, "AccountApiRepo", "account-api");

        var accountApiExecutionRole = new Role(this, "AccountApiExecutionRole", new RoleProps
        {
            AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com")
        });

        accountApiExecutionRole.AddManagedPolicy(ManagedPolicy.FromManagedPolicyArn(this, "TaskExecutionPolicy",
            "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"));

        var accountApiTaskRole = new Role(this, "AccountApiTaskRole", new RoleProps
        {
            AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com")
        });
        
        var commitHash = System.Environment.GetEnvironmentVariable("COMMIT_HASH");

        var taskDefinition = new FargateTaskDefinition(this, "AccountApiDefinition", new FargateTaskDefinitionProps
        {
            MemoryLimitMiB = 512,
            RuntimePlatform = new RuntimePlatform
            {
                CpuArchitecture = CpuArchitecture.X86_64,
                OperatingSystemFamily = OperatingSystemFamily.LINUX
            },
            ExecutionRole = accountApiExecutionRole,
            TaskRole = accountApiTaskRole
        });
        taskDefinition.AddContainer("api", new ContainerDefinitionOptions
        {
            Image = ContainerImage.FromEcrRepository(repository, commitHash ?? "latest"),
            PortMappings = new[]
            {
                new PortMapping
                {
                    ContainerPort = 8080,
                    Protocol = Protocol.TCP
                }
            },
            ContainerName = "account-api",
            Environment = new Dictionary<string, string>
            {
                {
                    "OtlpEndpoint",
                    "http://127.0.0.1:4318/v1/traces"
                },
                { "OtlpUseHttp", "Y" },
                { "Environment", "dev" },
                { "ServiceDiscovery__MyUrl", "" },
                { "ServiceDiscovery__ServiceName", "" },
                { "ServiceDiscovery__ConsulServiceEndpoint", "" },
                { "Messaging__BusName", bus.EventBusName },
                { "Auth__Issuer", "https://plantbasedpizza.com" },
                { "Auth__Audience", "https://plantbasedpizza.com" },
                { "SERVICE_NAME", "AccountWebApi" },
                { "ENV", "dev" }
            },
            Secrets = new Dictionary<string, Secret>(1)
            {
                { "DatabaseConnection", Secret.FromSsmParameter(databaseConnectionParam) },
                { "Auth__Key", Secret.FromSsmParameter(jwtKeyParam) }
            },
            Logging = LogDrivers.Firelens(new FireLensLogDriverProps
            {
                Options = new Dictionary<string, string>
                {
                    { "Name", "datadog" },
                    { "Host", "http-intake.logs.datadoghq.eu" },
                    { "dd_service", "AccountWebApi" },
                    { "dd_source", "aspnet" },
                    { "dd_message_key", "log" },
                    { "dd_tags", "project:fluentbit" },
                    { "TLS", "on" },
                    { "provider", "ecs" }
                },
                SecretOptions = new Dictionary<string, Secret>(1)
                {
                    { "apikey", Secret.FromSsmParameter(ddApiKeyParam) }
                }
            })
        });
        taskDefinition.AddContainer("datadog-agent", new ContainerDefinitionOptions
        {
            Image = ContainerImage.FromRegistry("public.ecr.aws/datadog/agent:latest"),
            PortMappings = new[]
            {
                new PortMapping
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

        var accountApiService = new FargateService(this, "AccountApiService", new FargateServiceProps
        {
            Cluster = cluster,
            TaskDefinition = taskDefinition,
            DesiredCount = 1,
            AssignPublicIp = true
        });

        var listener = new ApplicationListener(this, "AccountApiListener", new ApplicationListenerProps
        {
            LoadBalancer = sharedAlb,
            Port = 80,
            DefaultAction = ListenerAction.FixedResponse(404)
        });

        var targetGroup = new ApplicationTargetGroup(this, "AccountApiTargetGroup", new ApplicationTargetGroupProps
        {
            Port = 8080,
            Targets = new List<IApplicationLoadBalancerTarget>(1)
            {
                accountApiService
            }.ToArray(),
            HealthCheck = new HealthCheck
            {
                Port = "8080",
                Path = "/account/health",
                HealthyHttpCodes = "200-404"
            },
            Vpc = vpc
        });

        sharedListener.AddTargetGroups("ECS", new AddApplicationTargetGroupsProps
        {
            Conditions = new[] { ListenerCondition.PathPatterns(new[] { "/account/*" }) },
            Priority = 1,
            TargetGroups = new IApplicationTargetGroup[1] { targetGroup }
        });

        var recipeService = new WebService(this, "RecipeWebService", new ConstructProps(
            vpc,
            cluster,
            "RecipeApi",
            "/shared/dd-api-key",
            "/shared/jwt-key",
            "recipe-api",
            commitHash ?? "latest",
            8080,
            new Dictionary<string, string>
            {
                { "Messaging__BusName", bus.EventBusName },
                { "SERVICE_NAME", "RecipeWebApi" },
                { "ENV", "dev" }
            },
            new Dictionary<string, Secret>(1)
            {
                { "DatabaseConnection", Secret.FromSsmParameter(databaseConnectionParam) }
            },
            "arn:aws:elasticloadbalancing:eu-west-1:730335273443:loadbalancer/app/plant-based-pizza-shared-ingress/1c948325c1df4e86",
            "arn:aws:elasticloadbalancing:eu-west-1:730335273443:listener/app/plant-based-pizza-ingress/d99d1b57574af81c/396097df348029f2",
            "/recipes/health",
            "/recipes/*",
            50
        ));

        ddApiKeyParam.GrantRead(accountApiExecutionRole);
        databaseConnectionParam.GrantRead(accountApiExecutionRole);
        repository.GrantPull(accountApiExecutionRole);
        bus.GrantPutEventsTo(accountApiTaskRole);

        databaseConnectionParam.GrantRead(recipeService.ExecutionRole);
        bus.GrantPutEventsTo(recipeService.TaskRole);
    }
}