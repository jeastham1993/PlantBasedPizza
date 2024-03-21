using System.Text;
using System.Text.Json;
using PlantBasedPizza.LoyaltyPoints.IntegrationTest.ViewModels;

namespace PlantBasedPizza.LoyaltyPoints.IntegrationTest.Drivers;

public class LoyaltyPointsDriver
    {
        private static string BaseUrl = TestConstants.DefaultTestUrl;

        private readonly HttpClient _httpClient;

        public LoyaltyPointsDriver()
        {
            this._httpClient = new HttpClient();
        }

        public async Task AddLoyaltyPoints(string customerIdentifier, string orderIdentifier, decimal orderValue)
        {
            var url = $"{BaseUrl}/loyalty";
            var content = JsonSerializer.Serialize(new AddLoyaltyPointsCommand(){CustomerIdentifier = customerIdentifier, OrderIdentifier = orderIdentifier, OrderValue = orderValue});
            
            await this._httpClient.PostAsync(new Uri(url), new StringContent(content, Encoding.UTF8, "application/json")).ConfigureAwait(false);
        }

        public async Task<LoyaltyPointsDTO?> GetLoyaltyPoints(string customerIdentifier)
        {
            var url = $"{BaseUrl}/loyalty/{customerIdentifier}";
            
            var getResult = await this._httpClient.GetAsync(new Uri(url)).ConfigureAwait(false);

            return JsonSerializer.Deserialize<LoyaltyPointsDTO>(await getResult.Content.ReadAsStringAsync());
        }

        public async Task SpendLoyaltyPoints(string customerIdentifier, string orderIdentifier, int points)
        {
            var url = $"{BaseUrl}/loyalty/spend";
            
            var content = JsonSerializer.Serialize(new SpendLoyaltyPointsCommand(){CustomerIdentifier = customerIdentifier, OrderIdentifier = orderIdentifier, PointsToSpend = points});
            
            await this._httpClient.PostAsync(new Uri(url), new StringContent(content, Encoding.UTF8, "application/json")).ConfigureAwait(false);
        }
    }