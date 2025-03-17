using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.ServiceDiscovery;
using Amazon.CDK.AWS.SSM;
using AWS.Lambda.Powertools.Parameters;
using Constructs;
using PlantBasedPizza.Infra.Constructs;

namespace Infra;

public class DeliveryInfraStack : Stack
{
    internal DeliveryInfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
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
        var environment = System.Environment.GetEnvironmentVariable("ENV") ?? "test";
        var serviceName = "DeliveryService";

        var vpcLinkSecurityGroup =
            SecurityGroup.FromSecurityGroupId(this, "VpcLinkSecurityGroup", vpcLinkSecurityGroupId);
        
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

        var databaseConnectionParam = StringParameter.FromSecureStringParameterAttributes(this, "DatabaseParameter",
            new SecureStringParameterAttributes
            {
                ParameterName = "/shared/database-connection"
            });

        var cluster = new Cluster(this, "DeliveryServiceCluster", new ClusterProps
        {
            EnableFargateCapacityProviders = true,
            Vpc = vpc
        });

        var commitHash = System.Environment.GetEnvironmentVariable("COMMIT_HASH") ?? "latest";

        var deliveryApiService = new WebService(this, "DeliveryWebService", new ConstructProps(
            vpc,
            vpcLink,
            vpcLinkSecurityGroup.SecurityGroupId,
            httpApi,
            cluster,
            serviceName,
            environment,
            "/shared/dd-api-key",
            "/shared/jwt-key",
            "delivery-api",
            commitHash,
            8080,
            new Dictionary<string, string>
            {
                { "Messaging__BusName", bus.EventBusName },
                { "SERVICE_NAME", "DeliveryApi" }
            },
            new Dictionary<string, Secret>(1)
            {
                { "DatabaseConnection", Secret.FromSsmParameter(databaseConnectionParam) }
            },
            "/delivery/{proxy+}",
            "/delivery/health",
            serviceDiscoveryNamespace,
            "delivery.api",
            true
        ));

        var orderReadyForDeliveryQueueName = "Delivery-OrderReadyForDelivery";

        var orderSubmittedQueue = new EventQueue(this, orderReadyForDeliveryQueueName,
            new EventQueueProps(bus, serviceName, orderReadyForDeliveryQueueName, environment,
                "https://orders.plantbasedpizza/", "order.readyForDelivery.v1"));

        var worker = new BackgroundWorker(this, "DeliveryWorker", new BackgroundWorkerProps(
            new SharedInfrastructureProps(null, bus, internalHttpApi, serviceName, commitHash, environment),
            "../application",
            databaseConnectionParam,
            orderSubmittedQueue.Queue));

        databaseConnectionParam.GrantRead(deliveryApiService.ExecutionRole);
        bus.GrantPutEventsTo(deliveryApiService.TaskRole);
    }
}