using System.Collections.Generic;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.SSM;
using Constructs;
using PlantBasedPizza.Infra.Constructs;

namespace Infra;

public record BackgroundWorkerProps(SharedInfrastructureProps SharedProps, string ApplicationRoot, IQueue OrderSubmittedQueue);

public class BackgroundWorker : Construct
{
    public IFunction TakePaymentFunction { get; private set; }

    public BackgroundWorker(Construct scope, string id, BackgroundWorkerProps props) : base(scope, id)
    {
        var queueWorkerEnvVars = new Dictionary<string, string>()
        {
            { "Messaging__BusName", props.SharedProps.Bus.EventBusName },
        };
        var serviceName = "LoyaltyWorker";

        this.TakePaymentFunction = new QueueWorkerFunction(this, "TakePaymentFunction",
            new QueueWorkerFunctionProps(
                serviceName,
                "TakePaymentFunction",
                props.SharedProps.Environment,
                $"{props.ApplicationRoot}/BackgroundWorkers",
                "BackgroundWorkers::BackgroundWorkers.Functions_HandleOrderCompleted_Generated::HandleOrderCompleted",
                props.OrderSubmittedQueue,
                props.SharedProps.Vpc,
                props.SharedProps.CommitHash,
                queueWorkerEnvVars)).Function;
        
        props.SharedProps.Bus.GrantPutEventsTo(this.TakePaymentFunction);
    }
}