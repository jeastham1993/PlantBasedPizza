using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.ServiceDiscovery;
using AWS.Lambda.Powertools.Parameters;
using Constructs;
using PlantBasedPizza.Infra.Constructs;

namespace Infra;

public class OrderApiInfraStack : Stack
{
    internal OrderApiInfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        var parameterProvider = ParametersManager.SsmProvider
            .ConfigureClient(System.Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
                System.Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"),
                System.Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN"));

        var vpcIdParam = parameterProvider.Get("/shared/vpc-id");
        var namespaceId = parameterProvider.Get("/shared/namespace-id");
        var namespaceArn = parameterProvider.Get("/shared/namespace-arn");
        var namespaceName = parameterProvider.Get("/shared/namespace-name");
        var httpApiId = parameterProvider.Get("/shared/api-id");
        var internalHttpApiId = parameterProvider.Get("/shared/internal-api-id");
        var vpcLinkId = parameterProvider.Get("/shared/vpc-link-id");
        var vpcLinkSecurityGroupId = parameterProvider.Get("/shared/vpc-link-sg-id");
        var serviceName = "OrderService";

        var environment = System.Environment.GetEnvironmentVariable("ENV");

        var bus = EventBus.FromEventBusName(this, "SharedEventBus", "PlantBasedPizzaEvents");

        var vpc = Vpc.FromLookup(this, "MainVpc", new VpcLookupOptions
        {
            VpcId = vpcIdParam
        });

        var serviceDiscoveryNamespace = PrivateDnsNamespace.FromPrivateDnsNamespaceAttributes(this, "DNSNamespace",
            new PrivateDnsNamespaceAttributes
            {
                NamespaceId = namespaceId,
                NamespaceArn = namespaceArn,
                NamespaceName = namespaceName
            });
        var httpApi = HttpApi.FromHttpApiAttributes(this, "HttpApi", new HttpApiAttributes
        {
            HttpApiId = httpApiId
        });
        var internalHttpApi = HttpApi.FromHttpApiAttributes(this, "InternalHttpApi", new HttpApiAttributes
        {
            HttpApiId = internalHttpApiId
        });
        var vpcLink = VpcLink.FromVpcLinkAttributes(this, "HttpApiVpcLink",
            new VpcLinkAttributes
            {
                VpcLinkId = vpcLinkId,
                Vpc = vpc
            });

        var persistence = new Persistence(this, "Persistence", new PersistenceProps(environment));

        var cluster = new Cluster(this, "OrdersServiceCluster", new ClusterProps
        {
            Vpc = vpc,
            EnableFargateCapacityProviders = true
        });

        var commitHash = System.Environment.GetEnvironmentVariable("COMMIT_HASH") ?? "latest";

        var orderApiService = new WebService(this, "OrdersWebService", new ConstructProps(
            vpc,
            vpcLink,
            vpcLinkSecurityGroupId,
            httpApi,
            cluster,
            serviceName,
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
                { "Services__Recipes", $"https://{httpApi.ApiId}.execute-api.{Aws.REGION}.amazonaws.com" },
                { "Auth__PaymentApiKey", "12345" },
                { "DatabaseSettings__TableName", persistence.Table.TableName }
            },
            new Dictionary<string, Secret>(0),
            "/order/{proxy+}",
            "/order/health",
            serviceDiscoveryNamespace,
            "orders.api",
            true
        ));

        bus.GrantPutEventsTo(orderApiService.TaskRole);
        persistence.Table.GrantReadWriteData(orderApiService.TaskRole);

        var loyaltyPointsCheckedQueueName = "Orders-LoyaltyUpdatedQueue";
        var orderPreparingQueueName = "Orders-OrderPreparingQueue";
        var orderPrepCompleteQueueName = "Orders-OrderPrepCompleteQueue";
        var orderBakedQueueName = "Orders-OrderBakedQueue";
        var orderQualityCheckedQueueName = "Orders-OrderQualityCheckedQueue";
        var driverDeliveredOrderQueueName = "Orders-DriverDeliveredOrderQueue";
        var driverCollectedOrderQueueName = "Orders-DriverCollectedOrderQueue";
        var paymentsuccessfulQueueName = "Orders-PaymentSuccessfulQueue";

        var kitchenServiceSource = "https://kitchen.plantbasedpizza/";
        var deliveryServiceSource = "https://delivery.plantbasedpizza/";
        var paymentServiceSource = "https://payments.plantbasedpizza/";
        var loyaltyServiceSource = "https://orders.test.plantbasedpizza/";

        var loyaltyPointsQueue = new EventQueue(this, loyaltyPointsCheckedQueueName,
            new EventQueueProps(bus, serviceName, loyaltyPointsCheckedQueueName, environment,
                loyaltyServiceSource, "loyalty.customerLoyaltyPointsUpdated.v1"));
        var orderPreparingQueue = new EventQueue(this, orderPreparingQueueName,
            new EventQueueProps(bus, serviceName, orderPreparingQueueName, environment, kitchenServiceSource,
                "kitchen.orderPreparing.v1"));
        var orderPrepCompleteQueue = new EventQueue(this, orderPrepCompleteQueueName,
            new EventQueueProps(bus, serviceName, orderPrepCompleteQueueName, environment, kitchenServiceSource,
                "kitchen.orderPrepComplete.v1"));
        var orderBakedQueue = new EventQueue(this, orderBakedQueueName,
            new EventQueueProps(bus, serviceName, orderBakedQueueName, environment, kitchenServiceSource,
                "kitchen.orderBaked.v1"));
        var orderQualityCheckedQueue = new EventQueue(this, orderQualityCheckedQueueName,
            new EventQueueProps(bus, serviceName, orderQualityCheckedQueueName, environment, kitchenServiceSource,
                "kitchen.qualityChecked.v1"));
        var driverDeliveredOrderQueue = new EventQueue(this, driverDeliveredOrderQueueName,
            new EventQueueProps(bus, serviceName, driverDeliveredOrderQueueName, environment, deliveryServiceSource,
                "delivery.driverDeliveredOrder.v1"));
        var driverCollectedOrderQueue = new EventQueue(this, driverCollectedOrderQueueName,
            new EventQueueProps(bus, serviceName, driverCollectedOrderQueueName, environment, deliveryServiceSource,
                "delivery.driverCollectedOrder.v1"));
        var paymentSuccessfulQueue = new EventQueue(this, paymentsuccessfulQueueName,
            new EventQueueProps(bus, serviceName, paymentsuccessfulQueueName, environment, paymentServiceSource,
                "payments.paymentSuccessful.v1"));

        var backgroundWorker = new BackgroundWorker(this, "OrdersWorkerFunctions",
            new BackgroundWorkerProps(
                new SharedInfrastructureProps(null, bus, internalHttpApi, serviceName, commitHash, environment),
                "../application", persistence.Table, loyaltyPointsQueue.Queue, driverCollectedOrderQueue.Queue,
                driverDeliveredOrderQueue.Queue, orderBakedQueue.Queue, orderPrepCompleteQueue.Queue,
                orderPreparingQueue.Queue, orderQualityCheckedQueue.Queue, paymentSuccessfulQueue.Queue));
    }
}