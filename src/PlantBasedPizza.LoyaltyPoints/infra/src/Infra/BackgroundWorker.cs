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
    public IFunction OrderCompletedEventFunction { get; private set; }

    public BackgroundWorker(Construct scope, string id, BackgroundWorkerProps props) : base(scope, id)
    {
        var queueWorkerEnvVars = new Dictionary<string, string>()
        {
            { "Messaging__BusName", props.SharedProps.Bus.EventBusName },
            { "DATABASE_CONNECTION_PARAM_NAME", props.DatabaseConnectionParameter.ParameterName }
        };
        var serviceName = "LoyaltyWorker";

        this.OrderCompletedEventFunction = new QueueWorkerFunction(this, "OrderCompletedFunction",
            new QueueWorkerFunctionProps(
                serviceName,
                "OrderCompletedFunction",
                props.SharedProps.Environment,
                $"{props.ApplicationRoot}/BackgroundWorkers",
                "BackgroundWorkers::BackgroundWorkers.Functions_HandleOrderCompleted_Generated::HandleOrderCompleted",
                props.OrderSubmittedQueue,
                props.SharedProps.Vpc,
                props.SharedProps.CommitHash,
                queueWorkerEnvVars)).Function;
        
        props.DatabaseConnectionParameter.GrantRead(this.OrderCompletedEventFunction);
        props.SharedProps.Bus.GrantPutEventsTo(this.OrderCompletedEventFunction);
    }
}