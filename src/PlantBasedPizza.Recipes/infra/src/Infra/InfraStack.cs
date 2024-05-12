using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.SSM;
using Constructs;
using HealthCheck = Amazon.CDK.AWS.ECS.HealthCheck;
using Protocol = Amazon.CDK.AWS.ElasticLoadBalancingV2.Protocol;
using PlantBasedPizza.Infra.Constructs;

namespace Infra
{
    public class RecipeApiInfraStack : Stack
    {
        internal RecipeApiInfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var parameterProvider = AWS.Lambda.Powertools.Parameters.ParametersManager.SsmProvider
                .ConfigureClient(System.Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"), System.Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"), System.Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN"));

            var vpcIdParam = parameterProvider.Get("/shared/vpc-id");
            var albArnParam = parameterProvider.Get("/shared/alb-arn");
            var albListener = parameterProvider.Get("/shared/alb-listener");
            var internalAlbArnParam = parameterProvider.Get("/shared/internal-alb-arn");
            var internalAlbListener = parameterProvider.Get("/shared/internal-alb-listener");

            var bus = EventBus.FromEventBusName(this, "SharedEventBus", "PlantBasedPizzaEvents");

            var vpc = Vpc.FromLookup(this, "MainVpc", new VpcLookupOptions()
            {
                VpcId = vpcIdParam,
            });

            var databaseConnectionParam = StringParameter.FromSecureStringParameterAttributes(this, "DatabaseParameter",
                new SecureStringParameterAttributes()
                {
                    ParameterName = "/shared/database-connection"
                });

            var cluster = new Cluster(this, "RecipeServiceCluster", new ClusterProps(){
                Vpc = vpc,
                EnableFargateCapacityProviders = true,
            });
        
            var commitHash = System.Environment.GetEnvironmentVariable("COMMIT_HASH") ?? "latest";

            var recipeService = new WebService(this, "RecipeWebService", new ConstructProps(
                vpc,
                cluster,
                "RecipeApi",
                "/shared/dd-api-key",
                "/shared/jwt-key",
                "recipe-api",
                commitHash,
                8080,
                new Dictionary<string, string>
                {
                    { "Messaging__BusName", bus.EventBusName },
                    { "SERVICE_NAME", "RecipeWebApi" }
                },
                new Dictionary<string, Secret>(1)
                {
                    { "DatabaseConnection", Secret.FromSsmParameter(databaseConnectionParam) }
                },
                albArnParam,
                albListener,
                "/recipes/health",
                "/recipes/*",
                51,
                internalAlbArnParam,
                internalAlbListener
            ));

            databaseConnectionParam.GrantRead(recipeService.ExecutionRole);
            bus.GrantPutEventsTo(recipeService.TaskRole);
        }
    }
}
