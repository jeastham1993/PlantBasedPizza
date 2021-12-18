using Amazon.DynamoDBv2.Model;
using PlantBasedPizza.Kitchen.Core.Entities;

namespace PlantBasedPizza.Kitchen.Infrastructure.Extensions;

public static class KitchenRequestExtensions
{
    public static Dictionary<string, AttributeValue> AsAttributeMap(this KitchenRequest request)
    {
        var attributes = new Dictionary<string, AttributeValue>();

        return attributes;
    }
}