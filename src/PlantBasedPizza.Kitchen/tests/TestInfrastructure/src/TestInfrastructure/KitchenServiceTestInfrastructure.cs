using Amazon.CDK;
using Amazon.CDK.AWS.SSM;
using Constructs;
using Infra;
using PlantBasedPizza.Infra.Constructs;
using EventBus = Amazon.CDK.AWS.Events.EventBus;
using EventBusProps = Amazon.CDK.AWS.Events.EventBusProps;

namespace TestInfrastructure;

public class KitchenServiceTestInfrastructure : Stack
{
    internal KitchenServiceTestInfrastructure(Construct scope, string id, ApplicationStackProps stackProps, IStackProps props = null) : base(scope, id, props)
    {
        var databaseConnectionParam = StringParameter.FromSecureStringParameterAttributes(this, "DatabaseParameter",
            new SecureStringParameterAttributes
            {
                ParameterName = "/shared/database-connection"
            });
        
        var bus = new EventBus(this, "KitchenApiTestBus", new EventBusProps()
        {
            EventBusName = $"test.kitchen.{stackProps.Version}"
        });

        var kitchenTestSource = "https://kitchen.test.plantbasedpizza/";
        var serviceName = "KitchenService";
        var orderSubmittedQueueName = "Kitchen-OrderSubmitted";
        
        var orderSubmittedQueue = new EventQueue(this, orderSubmittedQueueName, new EventQueueProps(bus, serviceName, orderSubmittedQueueName, stackProps.Version, kitchenTestSource, "order.orderConfirmed.v1"));
        
        var worker = new BackgroundWorker(this, "KitchenWorker", new BackgroundWorkerProps(
            new SharedInfrastructureProps(null, bus, null, serviceName, "int-test", stackProps.Version),
            "../../application",
            databaseConnectionParam,
            orderSubmittedQueue.Queue));

        var eventBus = new CfnOutput(this, "EBOutput", new CfnOutputProps()
        {
            ExportName = $"Kitchen-EventBusName-{stackProps.Version}",
            Value = bus.EventBusName
        });
    }
}