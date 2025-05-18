﻿using MongoDB.Driver;
using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.Recipes.Infrastructure;
 
internal class RecipeRepository : IRecipeRepository
{
    private readonly IMongoCollection<Recipe?> _recipes;

    public RecipeRepository(MongoClient client)
    {
        var database = client.GetDatabase("PlantBasedPizza_Monolith");
        this._recipes = database.GetCollection<Recipe>("recipes");
    }
    
    public async Task<Recipe?> Retrieve(string recipeIdentifier)
    {
        FilterDefinition<Recipe?> queryBuilder = Builders<Recipe>.Filter.Eq(p => p.RecipeIdentifier, recipeIdentifier);

        var recipe = await this._recipes.Find(queryBuilder).FirstOrDefaultAsync();

        return recipe;
    }

    public async Task<IEnumerable<Recipe?>> List()
    {
        var recipes = await this._recipes.Find(p => true).ToListAsync();

        return recipes;
    }

    public async Task Add(Recipe? recipe)
    {
        await this._recipes.InsertOneAsync(recipe).ConfigureAwait(false);
    }

    public async Task Update(Recipe? recipe)
    {
        FilterDefinition<Recipe?> queryBuilder = Builders<Recipe>.Filter.Eq(ord => ord.RecipeIdentifier, recipe.RecipeIdentifier);

        await this._recipes.ReplaceOneAsync(
            queryBuilder,
            recipe);
    }
}