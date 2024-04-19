using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlantBasedPizza.Events;
using PlantBasedPizza.IntegrationTest.Helpers;
using PlantBasedPizza.LoyaltyPoints.IntegrationTest.LoyaltyClient;
using PlantBasedPizza.LoyaltyPoints.IntegrationTest.ViewModels;
using Serilog.Extensions.Logging;

namespace PlantBasedPizza.LoyaltyPoints.IntegrationTest.Drivers;

public class LoyaltyPointsDriver
    {
        private static string BaseUrl = TestConstants.DefaultTestUrl;

        private readonly HttpClient _httpClient;
        private readonly IEventPublisher _eventPublisher;
        private readonly Loyalty.LoyaltyClient _loyaltyClient;

        public LoyaltyPointsDriver()
        {
            this._httpClient = new HttpClient();
            this._httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TestTokenGenerator.GenerateTestTokenForRole("user"));
            
            var channel = GrpcChannel.ForAddress(TestConstants.InternalTestEndpoint);
            this._loyaltyClient = new Loyalty.LoyaltyClient(channel);

            _eventPublisher = new RabbitMQEventPublisher(new OptionsWrapper<RabbitMqSettings>(new RabbitMqSettings()
            {
                ExchangeName = "dev.loyalty",
                HostName = "localhost"
            }), new Logger<RabbitMQEventPublisher>(new SerilogLoggerFactory()), new RabbitMQConnection("localhost"));
        }

        public async Task AddLoyaltyPoints(string orderIdentifier, decimal orderValue)
        {
            await this._eventPublisher.Publish(new OrderCompletedIntegrationEventV1()
            {
                CustomerIdentifier = "user-account",
                OrderIdentifier = orderIdentifier,
                OrderValue = orderValue
            });
        }

        public async Task<LoyaltyPointsDto?> GetLoyaltyPointsInternal(string customerIdentifier)
        {
            // Delay to allow for message processing
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            var points = await this._loyaltyClient.GetCustomerLoyaltyPointsAsync(new GetCustomerLoyaltyPointsRequest()
            {
                CustomerIdentifier = customerIdentifier
            });

            return new LoyaltyPointsDto()
            {
                CustomerIdentifier = points.CustomerIdentifier,
                TotalPoints = Convert.ToDecimal(points.TotalPoints)
            };
        }

        public async Task<LoyaltyPointsDto?> GetLoyaltyPoints(string customerIdentifier)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            var url = $"{BaseUrl}/loyalty";
            
            var getResult = await this._httpClient.GetAsync(new Uri(url)).ConfigureAwait(false);

            return JsonSerializer.Deserialize<LoyaltyPointsDto>(await getResult.Content.ReadAsStringAsync());
        }
    }