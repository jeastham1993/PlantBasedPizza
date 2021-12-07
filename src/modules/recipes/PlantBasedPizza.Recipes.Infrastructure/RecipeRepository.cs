using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using PlantBasedPizza.Recipes.Core.Entities;

public class RecipeRepository : IRecipeRepository
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<Recipe> _recipes;

    public RecipeRepository(MongoClient client)
    {
        this._database = client.GetDatabase("PlantBasedPizza");
        this._recipes = this._database.GetCollection<Recipe>("recipes");
    }
    
    public async Task<Recipe> Retrieve(string recipeIdentifier)
    {
        var queryBuilder = Builders<Recipe>.Filter.Eq(p => p.RecipeIdentifier, recipeIdentifier);

        var recipe = await this._recipes.Find(queryBuilder).FirstOrDefaultAsync().ConfigureAwait(false);

        return recipe;
    }

    public async Task<IEnumerable<Recipe>> List()
    {
        var recipes = await this._recipes.Find(p => true).ToListAsync();

        return recipes;
    }

    public async Task Add(Recipe recipe)
    {
        await this._recipes.InsertOneAsync(recipe).ConfigureAwait(false);
    }

    public async Task Update(Recipe recipe)
    {
        var queryBuilder = Builders<Recipe>.Filter.Eq(ord => ord.RecipeIdentifier, recipe.RecipeIdentifier);

        await this._recipes.ReplaceOneAsync(queryBuilder, recipe);
    }
}
