using System.Text;
using System.Text.Json;
using Grpc.Net.Client;
using PlantBasedPizza.LoyaltyPoints.IntegrationTest.LoyaltyClient;
using PlantBasedPizza.LoyaltyPoints.IntegrationTest.ViewModels;

namespace PlantBasedPizza.LoyaltyPoints.IntegrationTest.Drivers;

public class LoyaltyPointsDriver
    {
        private static string BaseUrl = TestConstants.DefaultTestUrl;

        private readonly HttpClient _httpClient;
        private readonly Loyalty.LoyaltyClient _loyaltyClient;

        public LoyaltyPointsDriver()
        {
            this._httpClient = new HttpClient();

            var channel = GrpcChannel.ForAddress(TestConstants.InternalTestEndpoint);
            this._loyaltyClient = new Loyalty.LoyaltyClient(channel);
        }

        public async Task AddLoyaltyPoints(string customerIdentifier, string orderIdentifier, decimal orderValue)
        {
            var res = await this._loyaltyClient.AddLoyaltyPointsAsync(new AddLoyaltyPointsRequest()
            {
                CustomerIdentifier = customerIdentifier,
                OrderIdentifier = orderIdentifier,
                OrderValue = (double)orderValue
            });

            if (res is null)
            {
                throw new Exception($"Failure adding loyalty points");
            }
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