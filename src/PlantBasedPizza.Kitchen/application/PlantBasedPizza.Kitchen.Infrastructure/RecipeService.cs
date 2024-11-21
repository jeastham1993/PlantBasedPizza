using System.Text.Json;
using Dapr.Client;
using Microsoft.Extensions.Options;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Services;

namespace PlantBasedPizza.Kitchen.Infrastructure
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

        public async Task<RecipeAdapter> GetRecipe(string recipeIdentifier)
        {
            var recipeResult = await _httpClient.GetAsync($"http://{_serviceEndpoints.Recipes}/recipes/{recipeIdentifier}");

            var recipe = JsonSerializer.Deserialize<RecipeAdapter>(await recipeResult.Content.ReadAsStringAsync());

            return recipe;
        }
    }
}