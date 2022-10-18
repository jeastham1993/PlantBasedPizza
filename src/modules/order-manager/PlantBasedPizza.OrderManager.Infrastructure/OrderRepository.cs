using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using PlantBasedPizza.OrderManager.Core.Entites;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.OrderManager.Infrastructure.Extensions;
using PlantBasedPizza.Shared.Logging;

public class OrderRepository : IOrderRepository
{
    private readonly IObservabilityService _logger;
    private readonly AmazonDynamoDBClient _dynamoDbClient;

    public OrderRepository(IObservabilityService logger, AmazonDynamoDBClient dynamoDbClient)
    {
        this._logger = logger;
        this._dynamoDbClient = dynamoDbClient;
    }

    public async Task Add(Order order)
    {
        await this._dynamoDbClient.PutItemAsync(InfrastructureConstants.TableName,
            order.AsAttributeMap());
    }

    public async Task<Order> Retrieve(string orderIdentifier)
    {
        var getResult = await this._dynamoDbClient.GetItemAsync(InfrastructureConstants.TableName,
            new Dictionary<string, AttributeValue>(2)
            {
                { "PK", new AttributeValue($"ORDER#{orderIdentifier.ToUpper()}") },
                { "SK", new AttributeValue($"ORDER#{orderIdentifier.ToUpper()}") },
            });

        if (!getResult.IsItemSet)
        {
            return null;
        }
            
        var result = JsonConvert.DeserializeObject<Order>(getResult.Item["Data"].S);

        return result;
    }

    public async Task<List<Order>> GetAwaitingCollection()
    {
        var results = new List<Order>();
        
        var queryResults = await this._dynamoDbClient.QueryAsync(new QueryRequest()
        {
            TableName = InfrastructureConstants.TableName,
            IndexName = "GSI1",
            KeyConditionExpression = "GSI1PK = :PK and GSI1SK = :SK",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                {":PK", new AttributeValue { S =  $"PICKUPAWAITINGCOLLECTION" }},
                {":SK", new AttributeValue { S =  $"PICKUPAWAITINGCOLLECTION" }},
            }
        });

        foreach (var queryResult in queryResults.Items)
        {
            results.Add(JsonConvert.DeserializeObject<Order>(queryResult["Data"].S));
        }

        return results;
    }

    public async Task Update(Order order)
    {
        await this._dynamoDbClient.PutItemAsync(InfrastructureConstants.TableName,
            order.AsAttributeMap());
    }
}
