using System.Text;
using System.Text.Json;
using PlantBasedPizza.E2ETests.ViewModels;

namespace PlantBasedPizza.E2ETests.Drivers;

public class LoyaltyPointsDriver
    {
        private static string BaseUrl = TestConstants.LoyaltyTestUrl;

        private readonly HttpClient _httpClient;

        public LoyaltyPointsDriver()
        {
            this._httpClient = new HttpClient();
        }

        public async Task AddLoyaltyPoints(string customerIdentifier, string orderIdentifier, decimal orderValue)
        {
            var url = $"{BaseUrl}/loyalty";
            var content = JsonSerializer.Serialize(new AddLoyaltyPointsCommand(){CustomerIdentifier = customerIdentifier, OrderIdentifier = orderIdentifier, OrderValue = orderValue});
            
            var res = await this._httpClient.PostAsync(new Uri(url), new StringContent(content, Encoding.UTF8, "application/json")).ConfigureAwait(false);

            if (!res.IsSuccessStatusCode)
            {
                throw new Exception($"Invalid API response for add loyalty points {res.StatusCode}");
            }
        }

        public async Task<LoyaltyPointsDto?> GetLoyaltyPoints(string customerIdentifier)
        {
            var url = $"{BaseUrl}/loyalty/{customerIdentifier}";
            
            var getResult = await this._httpClient.GetAsync(new Uri(url)).ConfigureAwait(false);

            return JsonSerializer.Deserialize<LoyaltyPointsDto>(await getResult.Content.ReadAsStringAsync());
        }

        public async Task SpendLoyaltyPoints(string customerIdentifier, string orderIdentifier, int points)
        {
            var url = $"{BaseUrl}/loyalty/spend";
            
            var content = JsonSerializer.Serialize(new SpendLoyaltyPointsCommand(){CustomerIdentifier = customerIdentifier, OrderIdentifier = orderIdentifier, PointsToSpend = points});
            
            await this._httpClient.PostAsync(new Uri(url), new StringContent(content, Encoding.UTF8, "application/json")).ConfigureAwait(false);
        }
    }