using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using PlantBasedPizza.Recipes.Core.Entities;
using PlantBasedPizza.Recipes.Infrastructure;
using PlantBasedPizza.Recipes.Infrastructure.Extensions;
using PlantBasedPizza.Shared.Logging;

public class RecipeRepository : IRecipeRepository
{
    private readonly AmazonDynamoDBClient _dynamoDbClient;
    private readonly IObservabilityService _observability;

    public RecipeRepository(IObservabilityService observability, AmazonDynamoDBClient dynamoDbClient)
    {
        _observability = observability;
        _dynamoDbClient = dynamoDbClient;
    }
    
    public async Task<Recipe> Retrieve(string recipeIdentifier)
    {
        var getResult = await this._dynamoDbClient.GetItemAsync(InfrastructureConstants.TableName,
            new Dictionary<string, AttributeValue>(2)
            {
                { "PK", new AttributeValue($"RECIPES") },
                { "SK", new AttributeValue($"RECIPE#{recipeIdentifier.ToUpper()}") },
            });

        if (!getResult.IsItemSet)
        {
            return null;
        }
            
        var result = JsonConvert.DeserializeObject<Recipe>(getResult.Item["Data"].S);

        return result;
    }

    public async Task<IEnumerable<Recipe>> List()
    {
        var results = new List<Recipe>();
        
        var queryResults = await this._dynamoDbClient.QueryAsync(new QueryRequest()
        {
            TableName = InfrastructureConstants.TableName,
            IndexName = "GSI1",
            KeyConditionExpression = "GSI1PK = :PK and GSI1SK = :SK",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                {":PK", new AttributeValue { S =  $"RECIPES" }},
            }
        });

        foreach (var queryResult in queryResults.Items)
        {
            results.Add(JsonConvert.DeserializeObject<Recipe>(queryResult["Data"].S));
        }

        return results;
    }

    public async Task Add(Recipe recipe)
    {
        await this._dynamoDbClient.PutItemAsync(Environment.GetEnvironmentVariable("TABLE_NAME"),
            recipe.AsAttributeMap());
    }

    public async Task Update(Recipe recipe)
    {
        await this._dynamoDbClient.PutItemAsync(Environment.GetEnvironmentVariable("TABLE_NAME"),
            recipe.AsAttributeMap());
    }
}
