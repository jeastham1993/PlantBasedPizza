using System;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.AutoScaling;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.SSM;
using Constructs;
using PlantBasedPizza.Infra.Constructs;
using IBlockDevice = Amazon.CDK.AWS.AutoScaling.IBlockDevice;

namespace Infra;

public class AccountApiInfraStack : Stack
{
    internal AccountApiInfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        var parameterProvider = AWS.Lambda.Powertools.Parameters.ParametersManager.SsmProvider
            .ConfigureClient(System.Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"), System.Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"), System.Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN"));

        var vpcIdParam = parameterProvider.Get("/shared/vpc-id");
        var albArnParam = parameterProvider.Get("/shared/alb-arn");
        var albListener = parameterProvider.Get("/shared/alb-listener");
        
        var bus = EventBus.FromEventBusName(this, "SharedEventBus", "PlantBasedPizzaEvents");

        var vpc = Vpc.FromLookup(this, "MainVpc", new VpcLookupOptions
        {
            VpcId = vpcIdParam
        });

        var databaseConnectionParam = StringParameter.FromSecureStringParameterAttributes(this, "DatabaseParameter",
            new SecureStringParameterAttributes
            {
                ParameterName = "/shared/database-connection"
            });

        var cluster = new Cluster(this, "AccountServiceCluster", new ClusterProps
        {
            EnableFargateCapacityProviders = true,
            Vpc = vpc,

        });
        
        var commitHash = System.Environment.GetEnvironmentVariable("COMMIT_HASH") ?? "latest";

        var accountApiService = new WebService(this, "AccountWebService", new ConstructProps(
            vpc,
            cluster,
            "AccountApi",
            "/shared/dd-api-key",
            "/shared/jwt-key",
            "account-api",
            commitHash,
            8080,
            new Dictionary<string, string>
            {
                { "Messaging__BusName", bus.EventBusName },
                { "SERVICE_NAME", "AccountApi" },
            },
            new Dictionary<string, Secret>(1)
            {
                { "DatabaseConnection", Secret.FromSsmParameter(databaseConnectionParam) }
            },
            albArnParam,
            albListener,
            "/account/health",
            "/account/*",
            2
        ));

        databaseConnectionParam.GrantRead(accountApiService.ExecutionRole);
        bus.GrantPutEventsTo(accountApiService.TaskRole);
    }
}