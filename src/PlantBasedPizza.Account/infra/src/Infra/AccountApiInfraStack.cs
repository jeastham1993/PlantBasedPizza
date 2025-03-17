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

public class AccountApiInfraStack : Stack
{
    internal AccountApiInfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
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
        var vpcLinkId = parameterProvider.Get("/shared/vpc-link-id");
        var vpcLinkSecurityGroupId = parameterProvider.Get("/shared/vpc-link-sg-id");
        var environment = System.Environment.GetEnvironmentVariable("ENV") ?? "test";

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

        var cluster = new Cluster(this, "AccountServiceCluster", new ClusterProps
        {
            EnableFargateCapacityProviders = true,
            Vpc = vpc
        });

        var serviceName = "AccountApi";

        var commitHash = System.Environment.GetEnvironmentVariable("COMMIT_HASH") ?? "latest";

        var accountApiService = new WebService(this, "AccountWebService", new ConstructProps(
            vpc,
            vpcLink,
            vpcLinkSecurityGroupId,
            httpApi,
            cluster,
            serviceName,
            environment,
            "/shared/dd-api-key",
            "/shared/jwt-key",
            "account-api",
            commitHash,
            8080,
            new Dictionary<string, string>
            {
                { "Messaging__BusName", bus.EventBusName },
                { "SERVICE_NAME", "AccountApi" }
            },
            new Dictionary<string, Secret>(1)
            {
                { "DatabaseConnection", Secret.FromSsmParameter(databaseConnectionParam) }
            },
            "/account/{proxy+}",
            "/account/health",
            serviceDiscoveryNamespace,
            "account.api",
            true
        ));

        databaseConnectionParam.GrantRead(accountApiService.ExecutionRole);
        bus.GrantPutEventsTo(accountApiService.TaskRole);
    }
}