using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.SSM;
using Constructs;
using Infra;
using PlantBasedPizza.Infra.Constructs;
using EventBus = Amazon.CDK.AWS.Events.EventBus;
using EventBusProps = Amazon.CDK.AWS.Events.EventBusProps;

namespace TestInfrastructure;

public class OrdersApiTestInfrastructureStack : Stack
{
    internal OrdersApiTestInfrastructureStack(Construct scope, string id, ApplicationStackProps stackProps, IStackProps props = null) : base(scope, id, props)
    {
        var parameterProvider = AWS.Lambda.Powertools.Parameters.ParametersManager.SsmProvider
            .ConfigureClient(System.Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"), System.Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"), System.Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN"));

        var vpcIdParam = parameterProvider.Get("/shared/vpc-id");
        
        var bus = new EventBus(this, "OrdersApiTestBus", new EventBusProps()
        {
            EventBusName = $"test.orders.{stackProps.Version}"
        });
        
        var persistence = new Persistence(this, "Persistence", new PersistenceProps(stackProps.Version));

        var ordersTestSource = "https://orders.test.plantbasedpizza/";

        var preparingQueue = new EventQueue(this, "OrderPreparingQueue", new EventQueueProps(bus, "OrderPreparingQueue", stackProps.Version, ordersTestSource, "kitchen.orderPreparing.v1"));
        var prepCompleteQueue = new EventQueue(this, "OrderPrepCompleteQueue", new EventQueueProps(bus, "OrderPrepCompleteQueue", stackProps.Version, ordersTestSource, "kitchen.orderPrepComplete.v1"));
        var bakedQueue = new EventQueue(this, "OrderBakedQueue", new EventQueueProps(bus, "OrderBakedQueue", stackProps.Version, ordersTestSource, "kitchen.orderBaked.v1"));
        var orderQualityCheckedQueue = new EventQueue(this, "OrderQualityCheckedQueue", new EventQueueProps(bus, "OrderQualityCheckedQueue", stackProps.Version, ordersTestSource, "kitchen.orderQualityChecked.v1"));
        var driverDeliveredQueue = new EventQueue(this, "DriverDeliveredOrderQueue", new EventQueueProps(bus, "DriverDeliveredOrderQueue", stackProps.Version, ordersTestSource, "delivery.driverDeliveredOrder.v1"));
        var driverCollectedQueue = new EventQueue(this, "DriverCollectedOrderQueue", new EventQueueProps(bus, "DriverCollectedOrderQueue", stackProps.Version, ordersTestSource, "delivery.driverCollectedOrder.v1"));
        var loyaltyPointsQueue = new EventQueue(this, "LoyaltyUpdatedQueue", new EventQueueProps(bus, "LoyaltyUpdatedQueue", stackProps.Version, ordersTestSource, "loyalty.customerLoyaltyPointsUpdated.v1"));
        var paymentSuccessQueue = new EventQueue(this, "PaymentSuccessQueue", new EventQueueProps(bus, "PaymentSuccessQueue", stackProps.Version, ordersTestSource, "payments.paymentSuccessful.v1"));

        var backgroundWorker = new BackgroundWorker(this, "OrdersWorkerTestFunction",
            new BackgroundWorkerProps(new SharedInfrastructureProps(null, bus, null, "int-test", stackProps.Version),
                "../../application", persistence.Table, loyaltyPointsQueue.Queue, driverCollectedQueue.Queue, driverDeliveredQueue.Queue, bakedQueue.Queue, prepCompleteQueue.Queue, preparingQueue.Queue, orderQualityCheckedQueue.Queue, paymentSuccessQueue.Queue));

        var eventBus = new CfnOutput(this, "EBOutput", new CfnOutputProps()
        {
            ExportName = $"Orders-EventBusName-{stackProps.Version}",
            Value = bus.EventBusName
        });
    }
}