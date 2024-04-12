using System.Text.Json;
using PlantBasedPizza.OrderManager.Core.Services;
using Recipe = PlantBasedPizza.OrderManager.Core.Services.Recipe;

namespace PlantBasedPizza.OrderManager.Infrastructure
{
    public class RecipeService : IRecipeService
    {
        private readonly HttpClient _httpClient;

        public RecipeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Recipe> GetRecipe(string recipeIdentifier)
        {
            var recipeResult = await this._httpClient.GetAsync($"recipes/{recipeIdentifier}");

            var recipe = JsonSerializer.Deserialize<Recipe>(await recipeResult.Content.ReadAsStringAsync());

            return recipe;
        }
    }
}