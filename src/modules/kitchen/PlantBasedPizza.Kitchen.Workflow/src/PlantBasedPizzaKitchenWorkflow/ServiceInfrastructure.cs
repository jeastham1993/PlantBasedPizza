using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.SSM;
using Constructs;

namespace PlantBasedPizzaKitchenWorkflow
{
    public class ServiceInfrastructure  : Construct
    {
        public readonly Table KitchenRequestTable;
        public readonly Queue InboundOrderStorageQueue;

        public ServiceInfrastructure(Construct scope, string id, string cell) : base(scope, id)
        {
            var table = new Table(this, $"KitchenTable-{cell}", new TableProps()
            {
                TableName = $"KitchenTable-{cell}",
                BillingMode = BillingMode.PAY_PER_REQUEST,
                PartitionKey = new Attribute()
                {
                    Name = "PK",
                    Type = AttributeType.STRING
                },
                SortKey = new Attribute()
                {
                    Name = "SK",
                    Type = AttributeType.STRING
                },
            });
            
            
            var queue = new Queue(this, $"InboundKitchenRequestQueue-{cell}", new QueueProps()
            {
                QueueName = $"InboundKitchenRequestQueue-{cell}",
            });
            
            var secret = new StringParameter(this, $"InboundKitchenRequestQueueParameter-{cell}", new StringParameterProps()
            {
                ParameterName = $"/plant-based-pizza/kitchen/queue-url/{cell}",
                Description = "Kitchen Queue URL",
                StringValue = queue.QueueUrl
            });

            this.KitchenRequestTable = table;
            this.InboundOrderStorageQueue = queue;
        }
    }
}