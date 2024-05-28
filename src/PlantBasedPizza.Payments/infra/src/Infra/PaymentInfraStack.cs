using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.SSM;
using Constructs;
using PlantBasedPizza.Infra.Constructs;

namespace Infra;

public class PaymentInfraStack : Stack
{
    internal PaymentInfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        var parameterProvider = AWS.Lambda.Powertools.Parameters.ParametersManager.SsmProvider
            .ConfigureClient(System.Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"), System.Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"), System.Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN"));

        var vpcIdParam = parameterProvider.Get("/shared/vpc-id");
        var albArnParam = parameterProvider.Get("/shared/alb-arn");
        var albListener = parameterProvider.Get("/shared/alb-listener");
        var internalAlbArnParam = parameterProvider.Get("/shared/internal-alb-arn");
        var internalAlbListener = parameterProvider.Get("/shared/internal-alb-listener");
        var environment = System.Environment.GetEnvironmentVariable("ENV") ?? "test";
        
        var bus = EventBus.FromEventBusName(this, "SharedEventBus", "PlantBasedPizzaEvents");

        var vpc = Vpc.FromLookup(this, "MainVpc", new VpcLookupOptions
        {
            VpcId = vpcIdParam
        });
        
        var publicLoadBalancer = ApplicationLoadBalancer.FromLookup(this, "PublicSharedLoadBalancer",
            new ApplicationLoadBalancerLookupOptions()
            {
                LoadBalancerArn = albArnParam,
            });
        
        var internalLoadBalancer = ApplicationLoadBalancer.FromLookup(this, "SharedLoadBalancer",
            new ApplicationLoadBalancerLookupOptions()
            {
                LoadBalancerArn = internalAlbArnParam,
            });

        var databaseConnectionParam = StringParameter.FromSecureStringParameterAttributes(this, "DatabaseParameter",
            new SecureStringParameterAttributes
            {
                ParameterName = "/shared/database-connection"
            });

        var cluster = new Cluster(this, "LoyaltyServiceCluster", new ClusterProps
        {
            EnableFargateCapacityProviders = true,
            Vpc = vpc,
        });
        
        var commitHash = System.Environment.GetEnvironmentVariable("COMMIT_HASH") ?? "latest";

        var orderCompletedQueueName = "Payment-OrderSubmitted";
        
        var orderSubmittedQueue = new EventQueue(this, orderCompletedQueueName, new EventQueueProps(bus, orderCompletedQueueName, environment, "https://orders.plantbasedpizza/", "order.orderSubmitted.v1"));

        var worker = new BackgroundWorker(this, "PaymentWorker", new BackgroundWorkerProps(
            new SharedInfrastructureProps(null, bus, publicLoadBalancer, commitHash, environment),
            "../application",
            orderSubmittedQueue.Queue));
    }
}