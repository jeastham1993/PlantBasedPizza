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

        public async Task<LoyaltyPointsDto?> GetLoyaltyPoints(string customerIdentifier)
        {
            // Delay to allow async processing to catch up
            await Task.Delay(TimeSpan.FromSeconds(2));
            
            var url = $"{BaseUrl}/loyalty/{customerIdentifier}";
            
            var getResult = await this._httpClient.GetAsync(new Uri(url)).ConfigureAwait(false);

            return JsonSerializer.Deserialize<LoyaltyPointsDto>(await getResult.Content.ReadAsStringAsync());
        }
    }