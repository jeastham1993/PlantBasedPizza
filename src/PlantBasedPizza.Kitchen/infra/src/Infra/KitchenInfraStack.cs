using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.SSM;
using Constructs;
using PlantBasedPizza.Infra.Constructs;

namespace Infra;

public class KitchenInfraStack : Stack
{
    internal KitchenInfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        var parameterProvider = AWS.Lambda.Powertools.Parameters.ParametersManager.SsmProvider
            .ConfigureClient(System.Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"), System.Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"), System.Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN"));

        var vpcIdParam = parameterProvider.Get("/shared/vpc-id");
        var albArnParam = parameterProvider.Get("/shared/alb-arn");
        var albListener = parameterProvider.Get("/shared/alb-listener");
        var internalAlbArnParam = parameterProvider.Get("/shared/internal-alb-arn");
        var internalAlbListener = parameterProvider.Get("/shared/internal-alb-listener");
        var environment = System.Environment.GetEnvironmentVariable("ENV") ?? "test";
        
        var bus = EventBus.FromEventBusName(this, "SharedEventBus", "PlantBasedPizzaEvents");

        var vpc = Vpc.FromLookup(this, "MainVpc", new VpcLookupOptions
        {
            VpcId = vpcIdParam
        });
        
        var publicLoadBalancer = ApplicationLoadBalancer.FromLookup(this, "PublicSharedLoadBalancer",
            new ApplicationLoadBalancerLookupOptions()
            {
                LoadBalancerArn = albArnParam,
            });
        
        var internalLoadBalancer = ApplicationLoadBalancer.FromLookup(this, "SharedLoadBalancer",
            new ApplicationLoadBalancerLookupOptions()
            {
                LoadBalancerArn = internalAlbArnParam,
            });

        var databaseConnectionParam = StringParameter.FromSecureStringParameterAttributes(this, "DatabaseParameter",
            new SecureStringParameterAttributes
            {
                ParameterName = "/shared/database-connection"
            });

        var cluster = new Cluster(this, "KitchenServiceCluster", new ClusterProps
        {
            EnableFargateCapacityProviders = true,
            Vpc = vpc,
        });
        
        var commitHash = System.Environment.GetEnvironmentVariable("COMMIT_HASH") ?? "latest";

        var kitchenApiService = new WebService(this, "KitchenWebService", new ConstructProps(
            vpc,
            cluster,
            "KitchenApi",
            environment,
            "/shared/dd-api-key",
            "/shared/jwt-key",
            "kitchen-api",
            commitHash,
            8080,
            new Dictionary<string, string>
            {
                { "Messaging__BusName", bus.EventBusName },
                { "SERVICE_NAME", "KitchenApi" },
            },
            new Dictionary<string, Secret>(1)
            {
                { "DatabaseConnection", Secret.FromSsmParameter(databaseConnectionParam) }
            },
            albArnParam,
            albListener,
            "/kitchen/health",
            "/kitchen/*",
            32
        ));

        var orderSubmittedQueueName = "Kitchen-OrderSubmitted";
        
        var orderSubmittedQueue = new EventQueue(this, orderSubmittedQueueName, new EventQueueProps(bus, orderSubmittedQueueName, environment, "https://orders.plantbasedpizza/", "order.orderSubmitted.v1"));

        var worker = new BackgroundWorker(this, "KitchenWorker", new BackgroundWorkerProps(
            new SharedInfrastructureProps(null, bus, publicLoadBalancer, commitHash, environment),
            "../application",
            databaseConnectionParam,
            orderSubmittedQueue.Queue));
        
        databaseConnectionParam.GrantRead(kitchenApiService.ExecutionRole);
        bus.GrantPutEventsTo(kitchenApiService.TaskRole);
    }
}