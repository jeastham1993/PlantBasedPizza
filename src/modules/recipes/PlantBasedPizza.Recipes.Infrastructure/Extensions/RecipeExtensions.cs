using System.Text.Json;
using Amazon.DynamoDBv2.Model;
using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.Recipes.Infrastructure.Extensions;

public static class RecipeExtensions
{
    public static Dictionary<string, AttributeValue> AsAttributeMap(this Recipe request)
    {
        var attributeMap = new Dictionary<string, AttributeValue>(4)
        {
            { "PK", new AttributeValue($"RECIPES") },
            { "SK", new AttributeValue($"RECIPE#{request.RecipeIdentifier.ToUpper()}") },
            { "Type", new AttributeValue("Recipe") },
            { "Data", new AttributeValue(JsonSerializer.Serialize(request)) },
        };

        return attributeMap;
    }
}