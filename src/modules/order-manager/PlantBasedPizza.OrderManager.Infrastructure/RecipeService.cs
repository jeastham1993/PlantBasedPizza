using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Recipes.Core.Entities;
using Recipe = PlantBasedPizza.OrderManager.Core.ViewModels.Recipe;

namespace PlantBasedPizza.OrderManager.Infrastructure
{
    public class RecipeService : IRecipeService
    {
        private readonly IRecipeRepository _recipes;

        public RecipeService(IRecipeRepository recipes)
        {
            _recipes = recipes;
        }

        public async Task<Recipe> GetRecipe(string recipeIdentifier)
        {
            var recipe = await this._recipes.Retrieve(recipeIdentifier);

            return new Recipe()
            {
                Price = recipe.Price,
                ItemName = recipe.Name,
            };
        }
    }
}