using System.Text;
using System.Text.Json;
using PlantBasedPizza.FitnessFunctions.ViewModels;

namespace PlantBasedPizza.FitnessFunctions.Drivers
{
    public class RecipeDriver
    {
        private static string BaseUrl = TestConstants.DefaultTestUrl;

        private readonly HttpClient _httpClient;

        public RecipeDriver()
        {
            this._httpClient = new HttpClient();
        }
        
        public async Task<List<Recipe>> All()
        {
            var result = await this._httpClient.GetAsync(new Uri($"{BaseUrl}/recipes")).ConfigureAwait(false);

            var recipes = JsonSerializer.Deserialize<List<Recipe>>(await result.Content.ReadAsStringAsync());

            return recipes;
        }
    }
}