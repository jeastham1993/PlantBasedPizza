using System.Text.Json;
using Dapr.Client;
using Microsoft.Extensions.Options;
using PlantBasedPizza.OrderManager.Core.Services;
using Recipe = PlantBasedPizza.OrderManager.Core.Services.Recipe;

namespace PlantBasedPizza.OrderManager.Infrastructure
{
    public class RecipeService : IRecipeService
    {
        private readonly HttpClient _httpClient;
        private readonly ServiceEndpoints _serviceEndpoints;

        public RecipeService(IOptions<ServiceEndpoints> endpoints)
        {
            _httpClient = DaprClient.CreateInvokeHttpClient();
            _serviceEndpoints = endpoints.Value;
        }

        public async Task<Recipe> GetRecipe(string recipeIdentifier)
        {
            var recipeResult = await this._httpClient.GetAsync($"http://{_serviceEndpoints.Recipes}/recipes/{recipeIdentifier}");

            var recipe = JsonSerializer.Deserialize<Recipe>(await recipeResult.Content.ReadAsStringAsync());

            return recipe;
        }
    }
}