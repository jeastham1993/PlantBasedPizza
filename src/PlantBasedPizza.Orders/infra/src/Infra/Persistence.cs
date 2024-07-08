using Amazon.CDK.AWS.DynamoDB;
using Constructs;

namespace Infra;

public record PersistenceProps(string Environment);

public class Persistence : Construct
{
    public Table Table { get; private set; }
    
    public Persistence(Construct scope, string id, PersistenceProps props) : base(scope, id)
    {
        this.Table = new Table(this, "OrdersTable", new TableProps
        {
            TableName = $"Orders-{props.Environment}",
            TableClass = TableClass.STANDARD,
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
        
        this.Table.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps
        {
            IndexName = "GSI1",
            NonKeyAttributes = new string[]
            {
                "Data"
            },
            ProjectionType = ProjectionType.INCLUDE,
            PartitionKey = new Attribute()
            {
                Name = "GSI1PK",
                Type = AttributeType.STRING
            },
            SortKey = new Attribute()
            {
                Name = "GSI1SK",
                Type = AttributeType.STRING
            },
        });
        
        this.Table.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps
        {
            IndexName = "GSI2",
            NonKeyAttributes = new string[]
            {
                "Data"
            },
            ProjectionType = ProjectionType.INCLUDE,
            PartitionKey = new Attribute()
            {
                Name = "GSI2PK",
                Type = AttributeType.STRING
            },
        });
    }
}