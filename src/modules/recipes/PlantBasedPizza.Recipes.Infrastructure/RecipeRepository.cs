using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using PlantBasedPizza.Recipes.Core.Entities;
using PlantBasedPizza.Shared.Logging;

public class RecipeRepository : IRecipeRepository
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<Recipe> _recipes;
    private readonly IObservabilityService _observability;

    public RecipeRepository(MongoClient client, IObservabilityService observability)
    {
        _observability = observability;
        this._database = client.GetDatabase("PlantBasedPizza");
        this._recipes = this._database.GetCollection<Recipe>("recipes");
    }
    
    public async Task<Recipe> Retrieve(string recipeIdentifier)
    {
        var queryBuilder = Builders<Recipe>.Filter.Eq(p => p.RecipeIdentifier, recipeIdentifier);

        this._observability.StartTraceSubsegment("Database query");

        var recipe = await this._observability.TraceMethodAsync("Find Recipe Database Search",
            async () => await this._recipes.Find(queryBuilder).FirstOrDefaultAsync());

        this._observability.EndTraceSubsegment();
        
        return recipe;
    }

    public async Task<IEnumerable<Recipe>> List()
    {
        this._observability.StartTraceSubsegment("Database query");

        var recipes = await this._observability.TraceMethodAsync("Find Recipes Database Search",
            async () => await this._recipes.Find(p => true).ToListAsync());

        this._observability.EndTraceSubsegment();

        return recipes;
    }

    public async Task Add(Recipe recipe)
    {
        this._observability.StartTraceSubsegment("Database query");this._observability.StartTraceSubsegment("Database query");

        await this._observability.TraceMethodAsync("Add Recipe",
            async () => await this._recipes.InsertOneAsync(recipe).ConfigureAwait(false));

        this._observability.EndTraceSubsegment();
    }

    public async Task Update(Recipe recipe)
    {
        this._observability.StartTraceSubsegment("Database query");
        
        var queryBuilder = Builders<Recipe>.Filter.Eq(ord => ord.RecipeIdentifier, recipe.RecipeIdentifier);

        await this._observability.TraceMethodAsync("Update Recipe",
            async () => await this._recipes.ReplaceOneAsync(queryBuilder, recipe));

        this._observability.EndTraceSubsegment();
    }
}
