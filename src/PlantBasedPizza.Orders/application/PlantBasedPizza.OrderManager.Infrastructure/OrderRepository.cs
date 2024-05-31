using System.Diagnostics;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Datadog.Trace;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class OrderRepository : IOrderRepository
{
    private readonly AmazonDynamoDBClient _ddbClient;
    private readonly DatabaseSettings _dbSettings;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(AmazonDynamoDBClient ddbClient, IOptions<DatabaseSettings> dbSettings, ILogger<OrderRepository> logger)
    {
        _ddbClient = ddbClient;
        _logger = logger;
        _dbSettings = dbSettings.Value;
    }

    public async Task Add(Order order)
    {
        var ddbAttributes = new Dictionary<string, AttributeValue>()
        {
            { "PK", new AttributeValue(order.CustomerIdentifier) },
            { "SK", new AttributeValue(order.OrderNumber) },
            { "Type", new AttributeValue("Order") },
            { "Data", new AttributeValue(JsonSerializer.Serialize(order)) },
            { "GSI2PK", new AttributeValue(order.OrderNumber) }
        };

        if (order.OrderType == OrderType.Pickup && order.AwaitingCollection)
        {
            ddbAttributes.Add("GSI1PK", new AttributeValue("AWAITINGCOLLECTION"));
            ddbAttributes.Add("GSI1SK", new AttributeValue(order.OrderNumber));
        }

        await this._ddbClient.PutItemAsync(new PutItemRequest(_dbSettings.TableName,
            ddbAttributes
            ));
    }

    public async Task<Order> Retrieve(string customerIdentifier, string orderIdentifier)
    {
        var order = await this._ddbClient.GetItemAsync(this._dbSettings.TableName,
            new Dictionary<string, AttributeValue>(2)
            {
                { "PK", new AttributeValue(customerIdentifier) },
                { "SK", new AttributeValue(orderIdentifier) },
            });

        if (!order.IsItemSet)
        {
            Tracer.Instance.ActiveScope.Span.SetTag("order.notFound", "true");
            
            throw new OrderNotFoundException(orderIdentifier);
        }
        
        this._logger.LogInformation("Found order:");
        this._logger.LogInformation(order.Item["Data"].S);

        return JsonSerializer.Deserialize<Order>(order.Item["Data"].S);
    }

    public async Task<Order> RetrieveByOrderId(string orderIdentifier)
    {
        var queryResult = await this._ddbClient.QueryAsync(new QueryRequest()
        {
            TableName = _dbSettings.TableName,
            IndexName = "GSI2",
            KeyConditionExpression = "GSI2PK = :v_PK",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":v_PK", new AttributeValue { S = orderIdentifier } }
            }
        });

        var orders = new List<Order>(queryResult.Count);

        foreach (var ddbItem in queryResult.Items)
        {
            orders.Add(JsonSerializer.Deserialize<Order>(ddbItem["Data"].S));
        }
        
        return orders.FirstOrDefault();
    }

    public async Task<bool> Exists(string customerIdentifier, string orderIdentifier)
    {
        try
        {
            await this.Retrieve(customerIdentifier, orderIdentifier);

            return true;
        }
        catch (OrderNotFoundException)
        {
            return false;
        }
    }

    public async Task<List<Order>> GetAwaitingCollection()
    {
        var queryResult = await this._ddbClient.QueryAsync(new QueryRequest()
        {
            TableName = _dbSettings.TableName,
            IndexName = "GSI1",
            KeyConditionExpression = "GSI1PK = :v_PK",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":v_PK", new AttributeValue { S = "AWAITINGCOLLECTION" } }
            }
        });

        var orders = new List<Order>(queryResult.Count);

        foreach (var ddbItem in queryResult.Items)
        {
            orders.Add(JsonSerializer.Deserialize<Order>(ddbItem["Data"].S));
        }
        
        return orders;
    }

    public async Task Update(Order order)
    {
        var ddbAttributes = new Dictionary<string, AttributeValue>()
        {
            { "PK", new AttributeValue(order.CustomerIdentifier) },
            { "SK", new AttributeValue(order.OrderNumber) },
            { "Type", new AttributeValue("Order") },
            { "Data", new AttributeValue(JsonSerializer.Serialize(order)) },
            { "GSI2PK", new AttributeValue(order.OrderNumber) }
        };

        if (order.OrderType == OrderType.Pickup && order.AwaitingCollection)
        {
            ddbAttributes.Add("GSI1PK", new AttributeValue("AWAITINGCOLLECTION"));
            ddbAttributes.Add("GSI1SK", new AttributeValue(order.OrderNumber));
        }

        await this._ddbClient.PutItemAsync(new PutItemRequest(_dbSettings.TableName,
            ddbAttributes
        ));
    }
}