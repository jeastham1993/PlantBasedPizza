using System.Net.Http.Headers;
using System.Text.Json;
using Dapr.Client;
using Grpc.Core;
using Grpc.Net.Client;
using PlantBasedPizza.IntegrationTest.Helpers;
using PlantBasedPizza.LoyaltyPoints.IntegrationTest.LoyaltyClient;
using PlantBasedPizza.LoyaltyPoints.IntegrationTest.ViewModels;

namespace PlantBasedPizza.LoyaltyPoints.IntegrationTest.Drivers;

public class LoyaltyPointsDriver
    {
        private static string BaseUrl = TestConstants.DefaultTestUrl;
        private readonly Metadata _grpcMetadata;

        private readonly HttpClient _httpClient;
        private readonly DaprClient _daprClient;
        private readonly Loyalty.LoyaltyClient _loyaltyClient;

        public LoyaltyPointsDriver()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TestTokenGenerator.GenerateTestTokenForRole("user"));
            
            var channel = GrpcChannel.ForAddress(TestConstants.InternalTestEndpoint);
            _loyaltyClient = new Loyalty.LoyaltyClient(channel);
            
            _grpcMetadata = new Metadata
            {
                { "dapr-app-id", "loyaltyinternal" }
            };

            _daprClient = new DaprClientBuilder()
                .UseGrpcEndpoint("http://localhost:5101")
                .Build();
        }

        public async Task AddLoyaltyPoints(string orderIdentifier, decimal orderValue)
        {
            await _daprClient.PublishEventAsync("public", "order.orderCompleted.v1", new OrderCompletedIntegrationEventV1()
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
            
            var points = await _loyaltyClient.GetCustomerLoyaltyPointsAsync(new GetCustomerLoyaltyPointsRequest()
            {
                CustomerIdentifier = "user-account"
            }, _grpcMetadata);

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
            
            var getResult = await _httpClient.GetAsync(new Uri(url)).ConfigureAwait(false);

            return JsonSerializer.Deserialize<LoyaltyPointsDto>(await getResult.Content.ReadAsStringAsync());
        }
    }