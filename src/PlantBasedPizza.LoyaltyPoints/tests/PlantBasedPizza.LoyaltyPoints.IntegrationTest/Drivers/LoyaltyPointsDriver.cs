using System.Net.Http.Headers;
using System.Text.Json;
using Dapr.Client;
using Grpc.Net.Client;
using PlantBasedPizza.Events;
using PlantBasedPizza.IntegrationTest.Helpers;
using PlantBasedPizza.LoyaltyPoints.IntegrationTest.LoyaltyClient;
using PlantBasedPizza.LoyaltyPoints.IntegrationTest.ViewModels;

namespace PlantBasedPizza.LoyaltyPoints.IntegrationTest.Drivers;

public class LoyaltyPointsDriver
    {
        private static string BaseUrl = TestConstants.DefaultTestUrl;

        private readonly HttpClient _httpClient;
        private readonly DaprClient _daprClient;
        private readonly Loyalty.LoyaltyClient _loyaltyClient;

        public LoyaltyPointsDriver()
        {
            this._httpClient = new HttpClient();
            this._httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TestTokenGenerator.GenerateTestTokenForRole("user"));
            
            var channel = GrpcChannel.ForAddress(TestConstants.InternalTestEndpoint);
            this._loyaltyClient = new Loyalty.LoyaltyClient(channel);

            this._daprClient = new DaprClientBuilder()
                .UseGrpcEndpoint("http://localhost:5101")
                .Build();
        }

        public async Task AddLoyaltyPoints(string orderIdentifier, decimal orderValue)
        {
            await this._daprClient.PublishEventAsync("public", "order.orderCompleted.v1", new OrderCompletedIntegrationEventV1()
            {
                CustomerIdentifier = "user-account",
                OrderIdentifier = orderIdentifier,
                OrderValue = orderValue
            });
        }

        public async Task<LoyaltyPointsDto?> GetLoyaltyPointsInternal()
        {
            // Delay to allow for message processing
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            var points = await this._loyaltyClient.GetCustomerLoyaltyPointsAsync(new GetCustomerLoyaltyPointsRequest()
            {
                CustomerIdentifier = "user-account"
            });

            return new LoyaltyPointsDto()
            {
                CustomerIdentifier = points.CustomerIdentifier,
                TotalPoints = Convert.ToDecimal(points.TotalPoints)
            };
        }

        public async Task<LoyaltyPointsDto?> GetLoyaltyPoints()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            var url = $"{BaseUrl}/loyalty";
            
            var getResult = await this._httpClient.GetAsync(new Uri(url)).ConfigureAwait(false);

            return JsonSerializer.Deserialize<LoyaltyPointsDto>(await getResult.Content.ReadAsStringAsync());
        }
    }