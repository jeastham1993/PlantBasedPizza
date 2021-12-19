using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using MongoDB.Driver;
using Newtonsoft.Json;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Infrastructure;
using PlantBasedPizza.Kitchen.Infrastructure.Extensions;
using PlantBasedPizza.OrderManager.Core.Entites;
using PlantBasedPizza.Shared.Logging;

public class KitchenRequestRepository : IKitchenRequestRepository
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<KitchenRequest> _kitchenRequests;
    private readonly AmazonDynamoDBClient _dynamoDbClient;
    private readonly IObservabilityService _observability;

    public KitchenRequestRepository(AmazonDynamoDBClient dynamoDbClient, IObservabilityService observability)
    {
        _dynamoDbClient = dynamoDbClient;
        _observability = observability;
    }

    public async Task AddNew(KitchenRequest kitchenRequest)
    {
        await this._dynamoDbClient.PutItemAsync(Environment.GetEnvironmentVariable("TABLE_NAME"),
            kitchenRequest.AsAttributeMap());
    }

    public async Task Update(KitchenRequest kitchenRequest)
    {
        await this._dynamoDbClient.PutItemAsync(Environment.GetEnvironmentVariable("TABLE_NAME"),
            kitchenRequest.AsAttributeMap());
    }

    public async Task<KitchenRequest> Retrieve(string orderIdentifier)
    {
        var getResult = await this._dynamoDbClient.GetItemAsync(InfrastructureConstants.TableName,
            new Dictionary<string, AttributeValue>(2)
            {
                { "PK", new AttributeValue($"ORDER#{orderIdentifier.ToUpper()}") },
                { "SK", new AttributeValue($"KITCHEN#{orderIdentifier.ToUpper()}") },
            });

        if (!getResult.IsItemSet)
        {
            throw new Exception($"Order {orderIdentifier} not found");
        }
            
        var result = JsonConvert.DeserializeObject<KitchenRequest>(getResult.Item["Data"].S);

        return result;
    }

    public async Task<IEnumerable<KitchenRequest>> GetNew()
    {
        var queryResults = await this.queryByState(OrderState.NEW);

        var results = new List<KitchenRequest>();

        foreach (var queryResult in queryResults.Items)
        {
            results.Add(JsonConvert.DeserializeObject<KitchenRequest>(queryResult["Data"].S));
        }

        return results;
    }

    public async Task<IEnumerable<KitchenRequest>> GetPrep()
    {
        var queryResults = await this.queryByState(OrderState.PREPARING);

        var results = new List<KitchenRequest>();

        foreach (var queryResult in queryResults.Items)
        {
            results.Add(JsonConvert.DeserializeObject<KitchenRequest>(queryResult["Data"].S));
        }

        return results;
    }

    public async Task<IEnumerable<KitchenRequest>> GetBaking()
    {
        var queryResults = await this.queryByState(OrderState.BAKING);

        var results = new List<KitchenRequest>();

        foreach (var queryResult in queryResults.Items)
        {
            results.Add(JsonConvert.DeserializeObject<KitchenRequest>(queryResult["Data"].S));
        }

        return results;
    }

    public async Task<IEnumerable<KitchenRequest>> GetAwaitingQualityCheck()
    {
        var queryResults = await this.queryByState(OrderState.QUALITYCHECK);

        var results = new List<KitchenRequest>();

        foreach (var queryResult in queryResults.Items)
        {
            results.Add(JsonConvert.DeserializeObject<KitchenRequest>(queryResult["Data"].S));
        }

        return results;
    }

    private async Task<QueryResponse> queryByState(OrderState state)
    {
        return await this._dynamoDbClient.QueryAsync(new QueryRequest()
        {
            TableName = InfrastructureConstants.TableName,
            IndexName = "GSI1",
            KeyConditionExpression = "GSI1PK = :PK and GSI1SK = :SK",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                {":PK", new AttributeValue { S =  $"KITCHEN{state.ToString()}" }},
                {":SK", new AttributeValue { S =  $"KITCHEN{state.ToString()}" }},
            }
        });
        
    }
}
