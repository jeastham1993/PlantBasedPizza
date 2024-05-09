using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.SSM;
using Constructs;
using EventBus = Amazon.CDK.AWS.Events.EventBus;

namespace PlantBasedPizza.Infra.Constructs;

public record EventQueueProps(IEventBus Bus, string QueueName, string Environment, string EventSource, string DetailType);

public class EventQueue : Construct
{
    public IQueue Queue { get; private set; }
    
    public EventQueue(Construct scope, string id, EventQueueProps props) : base(scope, id)
    {
        var eventSource = props.EventSource;
        
        if (!props.EventSource.EndsWith("/"))
        {
            eventSource += "/";
        }
        
        this.Queue = new Queue(this, $"{props.QueueName}-{props.Environment}", new QueueProps()
        {
            QueueName = $"{props.QueueName}-{props.Environment}"
        });

        var rule = new Rule(this, $"{props.QueueName}Rule", new RuleProps()
        {
            EventBus = props.Bus
        });
        rule.AddEventPattern(new EventPattern()
        {
            Source = [eventSource],
            DetailType = [props.DetailType]
        });
        rule.AddTarget(new SqsQueue(this.Queue));
    }
}