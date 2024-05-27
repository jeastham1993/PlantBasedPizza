using System.Collections.Generic;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.SSM;
using Constructs;
using PlantBasedPizza.Infra.Constructs;

namespace Infra;

public record BackgroundWorkerProps(SharedInfrastructureProps SharedProps, string ApplicationRoot, IStringParameter DatabaseConnectionParameter, IQueue OrderSubmittedQueue);

public class BackgroundWorker : Construct
{
    public IFunction OrderSubmittedEventFunction { get; private set; }

    public BackgroundWorker(Construct scope, string id, BackgroundWorkerProps props) : base(scope, id)
    {
        var queueWorkerEnvVars = new Dictionary<string, string>()
        {
            { "Messaging__BusName", props.SharedProps.Bus.EventBusName },
            { "RedisConnectionString", "" },
            {
                "Services__Recipes",
                $"http://{(props.SharedProps.InternalAlb == null ? "" : props.SharedProps.InternalAlb.LoadBalancerDnsName)}"
            },
            { "DATABASE_CONNECTION_PARAM_NAME", props.DatabaseConnectionParameter.ParameterName }
        };
        var serviceName = "KitchenWorker";

        this.OrderSubmittedEventFunction = new QueueWorkerFunction(this, "OrderSubmittedFunction",
            new QueueWorkerFunctionProps(
                serviceName,
                "OrderSubmittedFunction",
                props.SharedProps.Environment,
                $"{props.ApplicationRoot}/BackgroundWorkers",
                "BackgroundWorkers::BackgroundWorkers.Functions_HandleOrderSubmitted_Generated::HandleOrderSubmitted",
                props.OrderSubmittedQueue,
                props.SharedProps.Vpc,
                props.SharedProps.CommitHash,
                queueWorkerEnvVars)).Function;
        
        props.DatabaseConnectionParameter.GrantRead(this.OrderSubmittedEventFunction);
        props.SharedProps.Bus.GrantPutEventsTo(this.OrderSubmittedEventFunction);
    }
}