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

public class DeliveryInfraStack : Stack
{
    internal DeliveryInfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
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

        var deliveryApiService = new WebService(this, "DeliveryWebService", new ConstructProps(
            vpc,
            cluster,
            "DeliveryApi",
            environment,
            "/shared/dd-api-key",
            "/shared/jwt-key",
            "delivery-api",
            commitHash,
            8080,
            new Dictionary<string, string>
            {
                { "Messaging__BusName", bus.EventBusName },
                { "SERVICE_NAME", "DeliveryApi" },
            },
            new Dictionary<string, Secret>(1)
            {
                { "DatabaseConnection", Secret.FromSsmParameter(databaseConnectionParam) }
            },
            albArnParam,
            albListener,
            "/delivery/health",
            "/delivery/*",
            72
        ));

        var orderReadyForDeliveryQueueName = "Delivery-OrderReadyForDelivery";
        
        var orderSubmittedQueue = new EventQueue(this, orderReadyForDeliveryQueueName, new EventQueueProps(bus, orderReadyForDeliveryQueueName, environment, "https://orders.plantbasedpizza/", "order.readyForDelivery.v1"));

        var worker = new BackgroundWorker(this, "DeliveryWorker", new BackgroundWorkerProps(
            new SharedInfrastructureProps(null, bus, publicLoadBalancer, commitHash, environment),
            "../application",
            databaseConnectionParam,
            orderSubmittedQueue.Queue));
        
        databaseConnectionParam.GrantRead(deliveryApiService.ExecutionRole);
        bus.GrantPutEventsTo(deliveryApiService.TaskRole);
    }
}