using Amazon.CDK;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;
using Amazon.CDK.AWS.SQS;
using Constructs;
using EventBus = Amazon.CDK.AWS.Events.EventBus;
using EventBusProps = Amazon.CDK.AWS.Events.EventBusProps;

namespace TestInfrastructure;

public class OrdersApiTestInfrastructureStack : Stack
{
    internal OrdersApiTestInfrastructureStack(Construct scope, string id, ApplicationStackProps stackProps, IStackProps props = null) : base(scope, id, props)
    {
        var bus = new EventBus(this, "OrdersApiTestBus", new EventBusProps()
        {
            EventBusName = $"test.orders.{stackProps.Version}"
        });

        var ordersTestSource = "https://orders.test.plantbasedpizza/";
        
        var preparingQueue = MapEventToTestQueue(bus, "OrderPreparingQueue", stackProps, ordersTestSource, "kitchen.orderPreparing.v1");
        var prepCompleteQueue = MapEventToTestQueue(bus, "OrderPrepCompleteQueue", stackProps, ordersTestSource, "kitchen.orderPrepComplete.v1");
        var bakedQueue = MapEventToTestQueue(bus, "OrderBakedQueue", stackProps, ordersTestSource, "kitchen.orderBaked.v1");
        var orderQualityCheckedQueue = MapEventToTestQueue(bus, "OrderQualityCheckedQueue", stackProps, "https://tests.orders/", "kitchen.orderQualityChecked.v1");
        var driverDeliveredQueue = MapEventToTestQueue(bus, "DriverDeliveredOrderQueue", stackProps, ordersTestSource, "delivery.driverDeliveredOrder.v1");
        var driverCollectedQueue = MapEventToTestQueue(bus, "DriverCollectedOrderQueue", stackProps, ordersTestSource, "delivery.driverCollectedOrder.v1");
        var loyaltyPointsUpdatedQueue = MapEventToTestQueue(bus, "LoyaltyUpdatedQueue", stackProps, ordersTestSource, "loyalty.customerLoyaltyPointsUpdated.v1");

        var eventBus = new CfnOutput(this, "EBOutput", new CfnOutputProps()
        {
            ExportName = $"EventBusName-{stackProps.Version}",
            Value = bus.EventBusName
        });
        var loyaltyUpdatedQueueOutput = new CfnOutput(this, "LoyaltyQueueUrl", new CfnOutputProps()
        {
            ExportName = $"LoyaltyPointsUpdatedQueueUrl-{stackProps.Version}",
            Value = loyaltyPointsUpdatedQueue.QueueUrl
        });
        var qualityCheckedQueueOutput = new CfnOutput(this, "QueueUrlOutput", new CfnOutputProps()
        {
            ExportName = $"OrderQualityCheckedQueueUrl-{stackProps.Version}",
            Value = orderQualityCheckedQueue.QueueUrl
        });
    }

    private Queue MapEventToTestQueue(EventBus bus, string queueName, ApplicationStackProps stackProps, string eventSource, string detailType)
    {
        if (!eventSource.EndsWith('/'))
        {
            eventSource += "/";
        }
        
        var queue = new Queue(this, $"{queueName}-{stackProps.Version}", new QueueProps()
        {
            QueueName = $"{queueName}-{stackProps.Version}"
        });

        var rule = new Rule(this, $"{queueName}Rule", new RuleProps()
        {
            EventBus = bus
        });
        rule.AddEventPattern(new EventPattern()
        {
            Source = [eventSource],
            DetailType = [detailType]
        });
        rule.AddTarget(new SqsQueue(queue));
        
        return queue;
    }
}