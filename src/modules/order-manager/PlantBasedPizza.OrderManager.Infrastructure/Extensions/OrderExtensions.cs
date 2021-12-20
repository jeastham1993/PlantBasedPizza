using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using PlantBasedPizza.OrderManager.Core.Entites;

namespace PlantBasedPizza.OrderManager.Infrastructure.Extensions;

public static class OrderExtensions
{
    public static Dictionary<string, AttributeValue> AsAttributeMap(this Order request)
    {
        var attributeMap = new Dictionary<string, AttributeValue>(4)
        {
            { "PK", new AttributeValue($"ORDER#{request.OrderIdentifier.ToUpper()}") },
            { "SK", new AttributeValue($"ORDER#{request.OrderIdentifier.ToUpper()}") },
            { "Type", new AttributeValue("Order") },
            { "Data", new AttributeValue(JsonConvert.SerializeObject(request)) },
        };

        if (request.OrderType == OrderType.PICKUP && request.AwaitingCollection)
        {
            attributeMap.Add("GSI1PK", new AttributeValue($"PICKUPAWAITINGCOLLECTION"));
            attributeMap.Add("GSI1SK", new AttributeValue($"PICKUPAWAITINGCOLLECTION"));
        }

        return attributeMap;
    }
}