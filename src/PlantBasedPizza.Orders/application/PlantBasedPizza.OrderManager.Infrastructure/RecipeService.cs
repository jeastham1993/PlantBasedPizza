using System.Text.Json;
using Microsoft.Extensions.Options;
using PlantBasedPizza.OrderManager.Core.Services;
using Recipe = PlantBasedPizza.OrderManager.Core.Services.Recipe;

namespace PlantBasedPizza.OrderManager.Infrastructure
{
    public class RecipeService : IRecipeService
    {
        private readonly HttpClient _httpClient;
        private readonly ServiceEndpoints _serviceEndpoints;

        public RecipeService(IHttpClientFactory clientFactory, IOptions<ServiceEndpoints> endpoints)
        {
            _httpClient = clientFactory.CreateClient("service-registry-http-client");
            _serviceEndpoints = endpoints.Value;
        }

        public async Task<Recipe> GetRecipe(string recipeIdentifier)
        {
            var recipeResult = await this._httpClient.GetAsync($"{_serviceEndpoints.Recipes}/recipes/{recipeIdentifier}");

            var recipe = JsonSerializer.Deserialize<Recipe>(await recipeResult.Content.ReadAsStringAsync());

            return recipe;
        }
    }
}