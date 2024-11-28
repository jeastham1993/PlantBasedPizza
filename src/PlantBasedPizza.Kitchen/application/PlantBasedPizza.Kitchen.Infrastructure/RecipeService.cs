using System.Diagnostics;
using System.Text.Json;
using Dapr.Client;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Services;

namespace PlantBasedPizza.Kitchen.Infrastructure
{
    public class RecipeService : IRecipeService
    {
        private readonly HttpClient _httpClient;
        private readonly ServiceEndpoints _serviceEndpoints;
        private readonly IDistributedCache _distributedCache;

        public RecipeService(IOptions<ServiceEndpoints> endpoints, IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
            _httpClient = DaprClient.CreateInvokeHttpClient();
            _serviceEndpoints = endpoints.Value;
        }

        public async Task<RecipeAdapter> GetRecipe(string recipeIdentifier)
        {
            var cachedRecipe = await this._distributedCache.GetAsync(recipeIdentifier);

            if (cachedRecipe != null)
            {
                Activity.Current?.AddTag("cache_hit", true);
                return JsonSerializer.Deserialize<RecipeAdapter>(cachedRecipe);
            }
            
            var recipeResult = await _httpClient.GetAsync($"http://{_serviceEndpoints.Recipes}/recipes/{recipeIdentifier}");

            var recipeData = await recipeResult.Content.ReadAsStringAsync();

            var recipe = JsonSerializer.Deserialize<RecipeAdapter>(recipeData);
            
            await _distributedCache.SetStringAsync(recipeIdentifier, recipeData);

            return recipe;
        }
    }
}