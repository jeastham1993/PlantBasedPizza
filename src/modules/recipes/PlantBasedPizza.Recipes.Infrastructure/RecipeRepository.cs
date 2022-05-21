using MongoDB.Bson;
using MongoDB.Driver;
using PlantBasedPizza.Recipes.Core.Entities;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;

public class RecipeRepository : IRecipeRepository
{
    private readonly IMongoCollection<Recipe> _collection;
    private readonly IObservabilityService _observability;

    public RecipeRepository(IObservabilityService observability, MongoClient mongoClient)
    {
        this._observability = observability;

        var database = mongoClient.GetDatabase(Environment.GetEnvironmentVariable("DATABASE_NAME"));
        this._collection = database.GetCollection<Recipe>("recipes");
    }
    
    public async Task<Recipe> Retrieve(string recipeIdentifier)
    {
        var filter = Builders<Recipe>.Filter.Eq(p => p.RecipeIdentifier, recipeIdentifier);

        var getResult = await this._collection.Find(filter).ToListAsync();

        if (!getResult.Any())
        {
            return null;
        }

        var result = getResult.FirstOrDefault();

        return result;
    }

    public async Task<IEnumerable<Recipe>> List()
    {
        var queryResults = await this._collection.Find(new BsonDocument()).ToListAsync();

        return queryResults;
    }

    public async Task Add(Recipe recipe)
    {
        await this._collection.InsertOneAsync(recipe);
    }

    public async Task Update(Recipe recipe)
    {
        var filter = Builders<Recipe>.Filter.Eq(p => p.RecipeIdentifier, recipe.RecipeIdentifier);

        await this._collection.FindOneAndReplaceAsync(filter, recipe);
    }
}
