using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.SSM;
using AWS.Lambda.Powertools.Parameters;
using Constructs;
using PlantBasedPizza.Infra.Constructs;

namespace Infra;

public class PaymentInfraStack : Stack
{
    internal PaymentInfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        var parameterProvider = ParametersManager.SsmProvider
            .ConfigureClient(System.Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
                System.Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"),
                System.Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN"));

        var vpcIdParam = parameterProvider.Get("/shared/vpc-id");
        var internalHttpApiId = parameterProvider.Get("/shared/internal-api-id");
        var environment = System.Environment.GetEnvironmentVariable("ENV") ?? "test";

        var bus = EventBus.FromEventBusName(this, "SharedEventBus", "PlantBasedPizzaEvents");

        var vpc = Vpc.FromLookup(this, "MainVpc", new VpcLookupOptions
        {
            VpcId = vpcIdParam
        });
        var internalHttpApi = HttpApi.FromHttpApiAttributes(this, "InternalHttpApi", new HttpApiAttributes
        {
            HttpApiId = internalHttpApiId
        });

        var databaseConnectionParam = StringParameter.FromSecureStringParameterAttributes(this, "DatabaseParameter",
            new SecureStringParameterAttributes
            {
                ParameterName = "/shared/database-connection"
            });

        var commitHash = System.Environment.GetEnvironmentVariable("COMMIT_HASH") ?? "latest";

        var orderCompletedQueueName = "Payment-OrderSubmitted";
        var serviceName = "PaymentService";

        var orderSubmittedQueue = new EventQueue(this, orderCompletedQueueName,
            new EventQueueProps(bus, serviceName, orderCompletedQueueName, environment,
                "https://orders.plantbasedpizza/", "order.orderSubmitted.v1"));

        var worker = new BackgroundWorker(this, "PaymentWorker", new BackgroundWorkerProps(
            new SharedInfrastructureProps(null, bus, internalHttpApi, serviceName, commitHash, environment),
            "../application",
            orderSubmittedQueue.Queue));
    }
}