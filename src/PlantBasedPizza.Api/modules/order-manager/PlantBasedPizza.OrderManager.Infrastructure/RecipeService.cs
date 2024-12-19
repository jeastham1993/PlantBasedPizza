using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Recipes.DataTransfer;
using Recipe = PlantBasedPizza.OrderManager.Core.ViewModels.Recipe;

namespace PlantBasedPizza.OrderManager.Infrastructure
{
    public class RecipeService : IRecipeService
    {
        private readonly RecipeDataTransferService _recipes;

        public RecipeService(RecipeDataTransferService recipes)
        {
            _recipes = recipes;
        }

        public async Task<Recipe> GetRecipe(string recipeIdentifier)
        {
            var recipe = await this._recipes.GetRecipeAsync(recipeIdentifier);

            return new Recipe()
            {
                Price = recipe.Price,
                ItemName = recipe.Name,
            };
        }
    }
}