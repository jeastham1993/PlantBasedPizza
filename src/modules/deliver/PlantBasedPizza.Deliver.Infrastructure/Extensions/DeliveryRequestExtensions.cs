using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using PlantBasedPizza.Deliver.Core.Entities;

namespace PlantBasedPizza.Deliver.Infrastructure.Extensions;

public static class DeliveryRequestExtensions
{
    public static Dictionary<string, AttributeValue> AsAttributeMap(this DeliveryRequest request)
    {
        var attributeMap = new Dictionary<string, AttributeValue>(4)
        {
            { "PK", new AttributeValue($"ORDER#{request.OrderIdentifier.ToUpper()}") },
            { "SK", new AttributeValue($"DELIVERY#{request.OrderIdentifier.ToUpper()}") },
            { "Type", new AttributeValue("DeliveryRequest") },
            { "Data", new AttributeValue(JsonConvert.SerializeObject(request)) },
        };

        if (!string.IsNullOrEmpty(request.Driver))
        {
            attributeMap.Add("GSI1PK", new AttributeValue($"DRIVER#{request.Driver.ToUpper()}"));
            attributeMap.Add("GSI1SK", new AttributeValue($"DRIVER#{request.Driver.ToUpper()}"));
        }
        else
        {
            attributeMap.Add("GSI2PK", new AttributeValue($"AWAITINGCOLLECTION"));
            attributeMap.Add("GSI2SK", new AttributeValue($"AWAITINGCOLLECTION"));
        }

        return attributeMap;
    }
}