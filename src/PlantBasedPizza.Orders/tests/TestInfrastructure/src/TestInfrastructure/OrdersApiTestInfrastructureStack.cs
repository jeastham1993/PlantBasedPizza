using Amazon.CDK;
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
        var databaseConnectionParam = StringParameter.FromSecureStringParameterAttributes(this, "DatabaseParameter",
            new SecureStringParameterAttributes
            {
                ParameterName = "/shared/database-connection"
            });
        
        var bus = new EventBus(this, "OrdersApiTestBus", new EventBusProps()
        {
            EventBusName = $"test.orders.{stackProps.Version}"
        });

        var ordersTestSource = "https://orders.test.plantbasedpizza/";

        var preparingQueue = new EventQueue(this, "OrderPreparingQueue", new EventQueueProps(bus, "OrderPreparingQueue", "dev", ordersTestSource, "kitchen.orderPreparing.v1"));
        var prepCompleteQueue = new EventQueue(this, "OrderPrepCompleteQueue", new EventQueueProps(bus, "OrderPrepCompleteQueue", "dev", ordersTestSource, "kitchen.orderPrepComplete.v1"));
        var bakedQueue = new EventQueue(this, "OrderBakedQueue", new EventQueueProps(bus, "OrderBakedQueue", "dev", ordersTestSource, "kitchen.orderBaked.v1"));
        var orderQualityCheckedQueue = new EventQueue(this, "OrderQualityCheckedQueue", new EventQueueProps(bus, "OrderQualityCheckedQueue", "dev", ordersTestSource, "kitchen.orderQualityChecked.v1"));
        var driverDeliveredQueue = new EventQueue(this, "DriverDeliveredOrderQueue", new EventQueueProps(bus, "DriverDeliveredOrderQueue", "dev", ordersTestSource, "delivery.driverDeliveredOrder.v1"));
        var driverCollectedQueue = new EventQueue(this, "DriverCollectedOrderQueue", new EventQueueProps(bus, "DriverCollectedOrderQueue", "dev", ordersTestSource, "delivery.driverCollectedOrder.v1"));
        var loyaltyPointsQueue = new EventQueue(this, "LoyaltyUpdatedQueue", new EventQueueProps(bus, "LoyaltyUpdatedQueue", "dev", ordersTestSource, "loyalty.customerLoyaltyPointsUpdated.v1"));

        var backgroundWorker = new BackgroundWorker(this, "OrdersWorkerTestFunction",
            new BackgroundWorkerProps(new SharedInfrastructureProps(null, bus, null, "int-test", "dev"),
                "../../application", databaseConnectionParam, loyaltyPointsQueue.Queue, driverCollectedQueue.Queue, driverDeliveredQueue.Queue, bakedQueue.Queue, prepCompleteQueue.Queue, preparingQueue.Queue, orderQualityCheckedQueue.Queue));

        var eventBus = new CfnOutput(this, "EBOutput", new CfnOutputProps()
        {
            ExportName = $"EventBusName-{stackProps.Version}",
            Value = bus.EventBusName
        });
    }
}