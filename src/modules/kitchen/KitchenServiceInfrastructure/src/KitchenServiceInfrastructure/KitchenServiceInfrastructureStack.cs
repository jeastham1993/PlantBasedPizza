using Amazon.CDK;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.SSM;
using Constructs;

namespace KitchenServiceInfrastructure
{
    public class KitchenServiceInfrastructureStack : Stack
    {
        internal KitchenServiceInfrastructureStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var queue = new Queue(this, "KitchenApiStorageQueue", new QueueProps()
            {
                QueueName = "KitchenQueue.fifo",
                Fifo = true
            });

            var secret = new StringParameter(this, "KitchenQueue", new StringParameterProps()
            {
                ParameterName = $"/plant-based-pizza/kitchen/queue-url",
                Description = "Kitchen Queue URL",
                StringValue = queue.QueueUrl
            });
        }
    }
}
