using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.DotNet;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.SSM;
using Constructs;
using PlantBasedPizza.Infra.Constructs;

namespace Infra;

public class OrderApiInfraStack : Stack
{
    internal OrderApiInfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        var parameterProvider = AWS.Lambda.Powertools.Parameters.ParametersManager.SsmProvider
            .ConfigureClient(System.Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"), System.Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"), System.Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN"));

        var vpcIdParam = parameterProvider.Get("/shared/vpc-id");
        var albArnParam = parameterProvider.Get("/shared/alb-arn");
        var albListener = parameterProvider.Get("/shared/alb-listener");
        var internalAlbArnParam = parameterProvider.Get("/shared/internal-alb-arn");
        var internalAlbListener = parameterProvider.Get("/shared/internal-alb-listener");
        
        var environment = System.Environment.GetEnvironmentVariable("ENV");
        
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

        var cluster = new Cluster(this, "OrdersServiceCluster", new ClusterProps
        {
            Vpc = vpc,
            EnableFargateCapacityProviders = true,
        });
        
        var commitHash = System.Environment.GetEnvironmentVariable("COMMIT_HASH") ?? "latest";

        var orderApiService = new WebService(this, "OrdersWebService", new ConstructProps(
            vpc,
            cluster,
            "OrdersApi",
            environment,
            "/shared/dd-api-key",
            "/shared/jwt-key",
            "orders-api",
            commitHash,
            8080,
            new Dictionary<string, string>
            {
                { "Messaging__BusName", bus.EventBusName },
                { "SERVICE_NAME", "OrderApi" },
                { "BUILD_VERSION", commitHash },
                { "RedisConnectionString", "" },
                { "Services__PaymentInternal", "http://localhost:1234"},
                { "Services__Recipes", $"http://{internalLoadBalancer.LoadBalancerDnsName}"},
                { "Auth__PaymentApiKey", "12345" },
            },
            new Dictionary<string, Secret>(1)
            {
                { "DatabaseConnection", Secret.FromSsmParameter(databaseConnectionParam) }
            },
            albArnParam,
            albListener,
            "/order/health",
            "/order/*",
            106
        ));
        
        databaseConnectionParam.GrantRead(orderApiService.ExecutionRole);
        bus.GrantPutEventsTo(orderApiService.TaskRole);

        var loyaltyPointsCheckedQueueName = "Orders-LoyaltyUpdatedQueue";
        var orderPreparingQueueName = "Orders-OrderPreparingQueue";
        var orderPrepCompleteQueueName = "Orders-OrderPrepCompleteQueue";
        var orderBakedQueueName = "Orders-OrderBakedQueue";
        var orderQualityCheckedQueueName = "Orders-OrderQualityCheckedQueue";
        var driverDeliveredOrderQueueName = "Orders-DriverDeliveredOrderQueue";
        var driverCollectedOrderQueueName = "Orders-DriverCollectedOrderQueue";
        
        var kitchenServiceSource = "https://kitchen.plantbasedpizza/";
        var deliveryServiceSource = "https://delivery.plantbasedpizza/";

        var loyaltyPointsQueue = new EventQueue(this, loyaltyPointsCheckedQueueName, new EventQueueProps(bus, loyaltyPointsCheckedQueueName, environment, "https://orders.test.plantbasedpizza/", "loyalty.customerLoyaltyPointsUpdated.v1"));
        var orderPreparingQueue = new EventQueue(this, orderPreparingQueueName, new EventQueueProps(bus, orderPreparingQueueName, environment, kitchenServiceSource, "kitchen.orderPreparing.v1"));
        var orderPrepCompleteQueue = new EventQueue(this, orderPrepCompleteQueueName, new EventQueueProps(bus, orderPrepCompleteQueueName, environment, kitchenServiceSource, "kitchen.orderPrepComplete.v1"));
        var orderBakedQueue = new EventQueue(this, orderBakedQueueName, new EventQueueProps(bus, orderBakedQueueName, environment, kitchenServiceSource, "kitchen.orderBaked.v1"));
        var orderQualityCheckedQueue = new EventQueue(this, orderQualityCheckedQueueName, new EventQueueProps(bus, orderQualityCheckedQueueName, environment, kitchenServiceSource, "kitchen.qualityChecked.v1"));
        var driverDeliveredOrderQueue = new EventQueue(this, driverDeliveredOrderQueueName, new EventQueueProps(bus, driverDeliveredOrderQueueName, environment, deliveryServiceSource, "delivery.driverDeliveredOrder.v1"));
        var driverCollectedOrderQueue = new EventQueue(this, driverCollectedOrderQueueName, new EventQueueProps(bus, driverCollectedOrderQueueName, environment, deliveryServiceSource, "delivery.driverCollectedOrder.v1"));

        var backgroundWorker = new BackgroundWorker(this, "OrdersWorkerFunctions",
            new BackgroundWorkerProps(new SharedInfrastructureProps(null, bus, publicLoadBalancer, commitHash, environment),
                "../application", databaseConnectionParam, loyaltyPointsQueue.Queue, driverCollectedOrderQueue.Queue, driverDeliveredOrderQueue.Queue, orderBakedQueue.Queue, orderPrepCompleteQueue.Queue, orderPreparingQueue.Queue, orderQualityCheckedQueue.Queue));
    }
}