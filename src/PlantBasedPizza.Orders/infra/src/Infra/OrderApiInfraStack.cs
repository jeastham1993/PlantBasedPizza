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
        
        var bus = EventBus.FromEventBusName(this, "SharedEventBus", "PlantBasedPizzaEvents");

        var vpc = Vpc.FromLookup(this, "MainVpc", new VpcLookupOptions
        {
            VpcId = vpcIdParam
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
            Vpc = vpc
        });
        
        var commitHash = System.Environment.GetEnvironmentVariable("COMMIT_HASH") ?? "latest";

        var loyaltyPointsCheckedQueueName = "Orders-LoyaltyUpdatedQueue";
        var orderPreparingQueueName = "Orders-OrderPreparingQueue";
        var orderPrepCompleteQueueName = "Orders-OrderPrepCompleteQueue";
        var orderBakedQueueName = "Orders-OrderBakedQueue";
        var orderQualityCheckedQueueName = "Orders-OrderQualityCheckedQueue";
        var driverDeliveredOrderQueueName = "Orders-DriverDeliveredOrderQueue";
        var driverCollectedOrderQueueName = "Orders-DriverCollectedOrderQueue";
        var kitchenServiceSource = "https://kitchen.plantbasedpizza/";

        var loyaltyPointsQueue = new EventQueue(this, loyaltyPointsCheckedQueueName, new EventQueueProps(bus, loyaltyPointsCheckedQueueName, "dev", "https://orders.test.plantbasedpizza/", "loyalty.customerLoyaltyPointsUpdated.v1"));
        var orderPreparingQueue = new EventQueue(this, orderPreparingQueueName, new EventQueueProps(bus, orderPreparingQueueName, "dev", kitchenServiceSource, "kitchen.orderPreparing.v1"));
        var orderPrepCompleteQueue = new EventQueue(this, orderPrepCompleteQueueName, new EventQueueProps(bus, orderPrepCompleteQueueName, "dev", kitchenServiceSource, "kitchen.orderPrepComplete.v1"));
        var orderBakedQueue = new EventQueue(this, orderBakedQueueName, new EventQueueProps(bus, orderBakedQueueName, "dev", kitchenServiceSource, "kitchen.orderBaked.v1"));
        var orderQualityCheckedQueue = new EventQueue(this, orderQualityCheckedQueueName, new EventQueueProps(bus, orderQualityCheckedQueueName, "dev", kitchenServiceSource, "kitchen.orderQualityChecked.v1"));
        var driverDeliveredOrderQueue = new EventQueue(this, driverDeliveredOrderQueueName, new EventQueueProps(bus, driverDeliveredOrderQueueName, "dev", "https://delivery.plantbasedpizza/", "delivery.driverDeliveredOrder.v1"));
        var driverCollectedOrderQueue = new EventQueue(this, driverCollectedOrderQueueName, new EventQueueProps(bus, driverCollectedOrderQueueName, "dev", "https://delivery.plantbasedpizza/", "delivery.driverCollectedOrder.v1"));

        var orderApiService = new WebService(this, "OrdersWebService", new ConstructProps(
            vpc,
            cluster,
            "OrdersApi",
            "/shared/dd-api-key",
            "/shared/jwt-key",
            "orders-api",
            commitHash,
            8080,
            new Dictionary<string, string>
            {
                { "Messaging__BusName", bus.EventBusName },
                { "SERVICE_NAME", "OrderApi" },
                { "BUILD_VERSION", "dev" },
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
        
        var orderWorkerService = new BackgroundService(this, "OrdersWorkerService", new BackgroundServiceConstructProps(
            vpc,
            cluster,
            "OrdersWorker",
            "/shared/dd-api-key",
            "/shared/jwt-key",
            "orders-worker",
            commitHash,
            8080,
            new Dictionary<string, string>
            {
                { "Messaging__BusName", bus.EventBusName },
                { "SERVICE_NAME", "OrdersWorker" },
                { "BUILD_VERSION", "dev" },
                { "RedisConnectionString", "" },
                { "Services__PaymentInternal", "http://localhost:1234"},
                { "Services__Recipes", $"http://{internalLoadBalancer.LoadBalancerDnsName}"},
                { "QueueConfiguration__OrderPreparingQueue", orderPreparingQueueName},
                { "QueueConfiguration__OrderPrepCompleteQueue", orderPrepCompleteQueueName},
                { "QueueConfiguration__OrderBakedQueue", orderBakedQueueName},
                { "QueueConfiguration__OrderQualityCheckedQueue", orderQualityCheckedQueueName},
                { "QueueConfiguration__LoyaltyPointsUpdatedQueue", loyaltyPointsCheckedQueueName},
                { "QueueConfiguration__DriverDeliveredOrderQueue", driverDeliveredOrderQueueName},
                { "QueueConfiguration__DriverCollectedOrderQueue", driverCollectedOrderQueueName},
                { "Auth__PaymentApiKey", "12345" },
            },
            new Dictionary<string, Secret>(1)
            {
                { "DatabaseConnection", Secret.FromSsmParameter(databaseConnectionParam) }
            },
            "/order/health"
        ));
        
        databaseConnectionParam.GrantRead(orderWorkerService.ExecutionRole);
        bus.GrantPutEventsTo(orderWorkerService.TaskRole);
        loyaltyPointsQueue.Queue.GrantConsumeMessages(orderWorkerService.TaskRole);
        orderPreparingQueue.Queue.GrantConsumeMessages(orderWorkerService.TaskRole);
        orderPrepCompleteQueue.Queue.GrantConsumeMessages(orderWorkerService.TaskRole);
        orderBakedQueue.Queue.GrantConsumeMessages(orderWorkerService.TaskRole);
        orderQualityCheckedQueue.Queue.GrantConsumeMessages(orderWorkerService.TaskRole);
        driverCollectedOrderQueue.Queue.GrantConsumeMessages(orderWorkerService.TaskRole);
        driverDeliveredOrderQueue.Queue.GrantConsumeMessages(orderWorkerService.TaskRole);
    }
}