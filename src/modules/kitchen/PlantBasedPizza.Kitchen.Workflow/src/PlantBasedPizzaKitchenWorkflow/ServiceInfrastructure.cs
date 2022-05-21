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

        public ServiceInfrastructure(Construct scope, string id) : base(scope, id)
        {
            var table = new Table(this, "KitchenTable", new TableProps()
            {
                TableName = "KitchenTable",
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
            
            
            var queue = new Queue(this, "InboundKitchenRequestQueue", new QueueProps()
            {
                QueueName = "InboundKitchenRequestQueue",
            });
            
            var secret = new StringParameter(this, "InboundKitchenRequestQueueParameter", new StringParameterProps()
            {
                ParameterName = $"/plant-based-pizza/kitchen/queue-url",
                Description = "Kitchen Queue URL",
                StringValue = queue.QueueUrl
            });

            this.KitchenRequestTable = table;
            this.InboundOrderStorageQueue = queue;
        }
    }
}