using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.Recipes.Infrastructure;

public class RecipeRepository : IRecipeRepository
{
    private readonly IMongoCollection<Recipe> _recipes;
    private readonly ILogger<RecipeRepository> _logger;

    public RecipeRepository(MongoClient client, ILogger<RecipeRepository> logger)
    {
        _logger = logger;
        var database = client.GetDatabase("PlantBasedPizza");
        this._recipes = database.GetCollection<Recipe>("recipes");
    }
    
    public async Task<Recipe> Retrieve(string recipeIdentifier)
    {
        var queryBuilder = Builders<Recipe>.Filter.Eq(p => p.RecipeIdentifier, recipeIdentifier);

        var recipe = await this._recipes.Find(queryBuilder).FirstOrDefaultAsync();

        if (recipe == null)
        {
            throw new RecipeNotFoundException();
        }

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

        await this._recipes.ReplaceOneAsync(
            queryBuilder,
            recipe);
    }

    public async Task SeedRecipes()
    {
        try
        {
            await this.Retrieve("marg");
            
            this._logger.LogInformation("Recipe exists, skipping database seed.");

            return;
        }
        catch (RecipeNotFoundException)
        {
            this._logger.LogInformation("Recipe not found, seeding data");
        }

        var marg = new Recipe(RecipeCategory.Pizza, "marg", "Margherita", 4.99M);
        marg.AddIngredient("Tomatoes", 1);
        marg.AddIngredient("Cheese", 6);

        var pepperoni = new Recipe(RecipeCategory.Pizza, "pepperoni", "Pepperoni", 10.99M);
        pepperoni.AddIngredient("Tomatoes", 1);
        pepperoni.AddIngredient("Cheese", 6);
        pepperoni.AddIngredient("Pepperoni", 20);
        
        var veggieDeluxe = new Recipe(RecipeCategory.Pizza, "veggie-deluxe", "Veggie Deluxe", 7.99M);
        veggieDeluxe.AddIngredient("Tomatoes", 1);
        veggieDeluxe.AddIngredient("Cheese", 6);
        veggieDeluxe.AddIngredient("Mushroom", 6);
        veggieDeluxe.AddIngredient("Red Peppers", 6);
        veggieDeluxe.AddIngredient("Green Peppers", 6);
        veggieDeluxe.AddIngredient("Olives", 12);
        
        var chickAint = new Recipe(RecipeCategory.Pizza, "chick-aint", "Chick-Aint", 10.99M);
        chickAint.AddIngredient("Tomatoes", 1);
        chickAint.AddIngredient("Cheese", 6);
        chickAint.AddIngredient("Chick-Aint", 12);
        veggieDeluxe.AddIngredient("Red Peppers", 6);
        
        var spicy = new Recipe(RecipeCategory.Pizza, "spicy", "Spicy Veggie", 9.99M);
        marg.AddIngredient("Tomatoes", 1);
        marg.AddIngredient("Cheese", 6);
        veggieDeluxe.AddIngredient("Mushroom", 6);
        veggieDeluxe.AddIngredient("Jalapenos", 12);

        var fries = new Recipe(RecipeCategory.Sides, "fries", "Fries", 3.99M);
        fries.AddIngredient("Potatoes", 4);
        
        await Add(marg);
        await Add(pepperoni);
        await Add(veggieDeluxe);
        await Add(chickAint);
        await Add(spicy);
        await Add(fries);

        var softDrinks = new[] { "Coca-Cola", "Fanta Orange", "Dr Pepper", "Water" };

        foreach (var drink in softDrinks)
        {
            await Add(new Recipe(RecipeCategory.Drinks, drink.ToLower().Replace(" ", "-"), drink, 1.50M));
        }
    }
}