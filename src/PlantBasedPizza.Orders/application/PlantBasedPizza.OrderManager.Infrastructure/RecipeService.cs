using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.OrderManager.Core.Services;
using Recipe = PlantBasedPizza.OrderManager.Core.Services.Recipe;

namespace PlantBasedPizza.OrderManager.Infrastructure
{
    public class RecipeService : IRecipeService
    {
        private readonly HttpClient _httpClient;
        private readonly ServiceEndpoints _serviceEndpoints;
        private readonly ILogger<RecipeService> _logger;

        public RecipeService(IHttpClientFactory clientFactory, IOptions<ServiceEndpoints> endpoints, ILogger<RecipeService> logger)
        {
            _logger = logger;

            _httpClient = clientFactory.CreateClient("recipe-service");

            _serviceEndpoints = endpoints.Value;
        }

        public async Task<Recipe> GetRecipe(string recipeIdentifier)
        {
            this._logger.LogInformation($"{_serviceEndpoints.Recipes}/recipes/{recipeIdentifier}");

            var recipeResult = await this._httpClient.GetAsync($"{_serviceEndpoints.Recipes}/recipes/{recipeIdentifier}");

            var recipe = JsonSerializer.Deserialize<Recipe>(await recipeResult.Content.ReadAsStringAsync());

            return recipe;
        }
    }
}