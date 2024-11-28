namespace PlantBasedPizza.OrderManager.Core.Services
{
    public interface IRecipeService
    {
        Task<Recipe?> GetRecipe(string recipeIdentifier);
    }
}