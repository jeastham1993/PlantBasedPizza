using Amazon.CDK;
using Amazon.CDK.AWS.SSM;
using Constructs;
using Infra;
using PlantBasedPizza.Infra.Constructs;
using EventBus = Amazon.CDK.AWS.Events.EventBus;
using EventBusProps = Amazon.CDK.AWS.Events.EventBusProps;

namespace TestInfrastructure;

public class LoyaltyServiceTestInfrastructure : Stack
{
    internal LoyaltyServiceTestInfrastructure(Construct scope, string id, ApplicationStackProps stackProps, IStackProps props = null) : base(scope, id, props)
    {
        var databaseConnectionParam = StringParameter.FromSecureStringParameterAttributes(this, "DatabaseParameter",
            new SecureStringParameterAttributes
            {
                ParameterName = "/shared/database-connection"
            });
        
        var bus = new EventBus(this, "LoyaltyTestBus", new EventBusProps()
        {
            EventBusName = $"test.loyalty.{stackProps.Version}"
        });

        var loyaltyTestSource = "https://loyalty.test.plantbasedpizza/";
        
        var orderCompletedQueueName = "Loyalty-OrderCompleted";
        
        var orderSubmittedQueue = new EventQueue(this, orderCompletedQueueName, new EventQueueProps(bus, orderCompletedQueueName, stackProps.Version, loyaltyTestSource, "order.orderCompleted.v1"));

        var worker = new BackgroundWorker(this, "LoyaltyWorker", new BackgroundWorkerProps(
            new SharedInfrastructureProps(null, bus, null, "int-test", stackProps.Version),
            "../../application",
            databaseConnectionParam,
            orderSubmittedQueue.Queue));

        var eventBus = new CfnOutput(this, "EBOutput", new CfnOutputProps()
        {
            ExportName = $"Kitche-EventBusName-{stackProps.Version}",
            Value = bus.EventBusName
        });
    }
}