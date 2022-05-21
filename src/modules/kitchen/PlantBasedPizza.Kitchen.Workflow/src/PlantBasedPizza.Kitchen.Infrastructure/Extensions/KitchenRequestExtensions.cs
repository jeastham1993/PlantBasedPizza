using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using PlantBasedPizza.Kitchen.Core.Entities;

namespace PlantBasedPizza.Kitchen.Infrastructure.Extensions;

public static class KitchenRequestExtensions
{
    public static Dictionary<string, AttributeValue> AsAttributeMap(this KitchenRequest request)
    {
        var attributeMap = new Dictionary<string, AttributeValue>(4)
        {
            { "PK", new AttributeValue($"ORDER#{request.OrderIdentifier.ToUpper()}") },
            { "SK", new AttributeValue($"KITCHEN#{request.OrderIdentifier.ToUpper()}") },
            { "Type", new AttributeValue("KitchenRequest") },
            { "Data", new AttributeValue(JsonConvert.SerializeObject(request)) },
        };
        
        attributeMap.Add("GSI1PK", new AttributeValue($"KITCHEN{request.OrderState.ToString()}"));
        attributeMap.Add("GSI1SK", new AttributeValue($"KITCHEN{request.OrderState.ToString()}"));

        return attributeMap;
    }
}