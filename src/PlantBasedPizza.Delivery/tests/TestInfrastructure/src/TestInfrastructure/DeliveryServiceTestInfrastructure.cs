using Amazon.CDK;
using Amazon.CDK.AWS.SSM;
using Constructs;
using Infra;
using PlantBasedPizza.Infra.Constructs;
using EventBus = Amazon.CDK.AWS.Events.EventBus;
using EventBusProps = Amazon.CDK.AWS.Events.EventBusProps;

namespace TestInfrastructure;

public class DeliveryServiceTestInfrastructure : Stack
{
    internal DeliveryServiceTestInfrastructure(Construct scope, string id, ApplicationStackProps stackProps, IStackProps props = null) : base(scope, id, props)
    {
        var databaseConnectionParam = StringParameter.FromSecureStringParameterAttributes(this, "DatabaseParameter",
            new SecureStringParameterAttributes
            {
                ParameterName = "/shared/database-connection"
            });
        
        var bus = new EventBus(this, "KitchenApiTestBus", new EventBusProps()
        {
            EventBusName = $"test.delivery.{stackProps.Version}"
        });

        var deliveryTestSource = "https://delivery.test.plantbasedpizza/";
        
        var orderReadyForDeliveryQueueName = "Delivery-OrderReadyForDelivery";
        
        var orderSubmittedQueue = new EventQueue(this, orderReadyForDeliveryQueueName, new EventQueueProps(bus, orderReadyForDeliveryQueueName, stackProps.Version, deliveryTestSource, "order.readyForDelivery.v1"));
        
        var worker = new BackgroundWorker(this, "DeliveryWorker", new BackgroundWorkerProps(
            new SharedInfrastructureProps(null, bus, null, "int-test", stackProps.Version),
            "../../application",
            databaseConnectionParam,
            orderSubmittedQueue.Queue));

        var eventBus = new CfnOutput(this, "EBOutput", new CfnOutputProps()
        {
            ExportName = $"Delivery-EventBusName-{stackProps.Version}",
            Value = bus.EventBusName
        });
    }
}