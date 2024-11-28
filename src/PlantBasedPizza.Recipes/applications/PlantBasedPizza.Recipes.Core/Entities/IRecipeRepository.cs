namespace PlantBasedPizza.Recipes.Core.Entities
{
    public interface IRecipeRepository
    {
        Task<Recipe?> Retrieve(string recipeIdentifier);

        Task<IEnumerable<Recipe>> List();

        Task Add(Recipe recipe);

        Task Update(Recipe recipe);
        
        Task<bool> Exists(Recipe recipe);
        
        // Seed data on startup, this wouldn't normally happen in a production application. 
        Task SeedRecipes();
    }
}