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
        
        var bus = EventBus.FromEventBusName(this, "SharedEventBus", "PlantBasedPizzaEvents");

        var vpc = Vpc.FromLookup(this, "MainVpc", new VpcLookupOptions
        {
            VpcId = vpcIdParam
        });

        var loadBalancer = ApplicationLoadBalancer.FromLookup(this, "SharedLoadBalancer",
            new ApplicationLoadBalancerLookupOptions()
            {
                LoadBalancerArn =
                    "arn:aws:elasticloadbalancing:eu-west-1:730335273443:loadbalancer/app/plant-based-pizza-shared-ingress/1c948325c1df4e86",
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
        var orderQualityCheckedQueueName = "Orders-OrderQualityCheckedQueue";

        var loyaltyPointsQueue = new EventQueue(this, loyaltyPointsCheckedQueueName, new EventQueueProps(bus, loyaltyPointsCheckedQueueName, "dev", "https://orders.test.plantbasedpizza/", "loyalty.customerLoyaltyPointsUpdated.v1"));
        var orderQualityCheckedQueue = new EventQueue(this, orderQualityCheckedQueueName, new EventQueueProps(bus, orderQualityCheckedQueueName, "dev", "https://tests.orders/", "kitchen.orderQualityChecked.v1"));

        var orderApiService = new WebService(this, "OrdersWebService", new ConstructProps(
            vpc,
            cluster,
            "OrdersApi",
            "/shared/dd-api-key",
            "/shared/jwt-key",
            "orders-api",
            commitHash ?? "latest",
            8080,
            new Dictionary<string, string>
            {
                { "Messaging__BusName", bus.EventBusName },
                { "SERVICE_NAME", "OrderApi" },
                { "BUILD_VERSION", "dev" },
                { "RedisConnectionString", "" },
                { "Services__PaymentInternal", "http://localhost:1234"},
                { "Services__Recipes", $"http://{loadBalancer.LoadBalancerDnsName}"},
            },
            new Dictionary<string, Secret>(1)
            {
                { "DatabaseConnection", Secret.FromSsmParameter(databaseConnectionParam) }
            },
            "arn:aws:elasticloadbalancing:eu-west-1:730335273443:loadbalancer/app/plant-based-pizza-shared-ingress/1c948325c1df4e86",
            "arn:aws:elasticloadbalancing:eu-west-1:730335273443:listener/app/plant-based-pizza-ingress/d99d1b57574af81c/396097df348029f2",
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
                { "Services__Recipes", $"http://{loadBalancer.LoadBalancerDnsName}"},
                { "QueueConfiguration__OrderQualityCheckedQueue", orderQualityCheckedQueueName},
                { "QueueConfiguration__LoyaltyPointsUpdatedQueue", loyaltyPointsCheckedQueueName}
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
        orderQualityCheckedQueue.Queue.GrantConsumeMessages(orderWorkerService.TaskRole);
    }
}