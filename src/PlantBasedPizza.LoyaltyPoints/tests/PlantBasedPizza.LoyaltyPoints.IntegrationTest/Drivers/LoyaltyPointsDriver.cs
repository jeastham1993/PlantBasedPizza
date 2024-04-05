using System.Text;
using System.Text.Json;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlantBasedPizza.Events;
using PlantBasedPizza.LoyaltyPoints.IntegrationTest.LoyaltyClient;
using PlantBasedPizza.LoyaltyPoints.IntegrationTest.ViewModels;
using Serilog.Extensions.Logging;

namespace PlantBasedPizza.LoyaltyPoints.IntegrationTest.Drivers;

public class LoyaltyPointsDriver
    {
        private static string BaseUrl = TestConstants.DefaultTestUrl;

        private readonly HttpClient _httpClient;
        private readonly IEventPublisher _eventPublisher;

        public LoyaltyPointsDriver()
        {
            this._httpClient = new HttpClient();

            var channel = GrpcChannel.ForAddress(TestConstants.InternalTestEndpoint);
            new Loyalty.LoyaltyClient(channel);

            _eventPublisher = new RabbitMQEventPublisher(new OptionsWrapper<RabbitMqSettings>(new RabbitMqSettings()
            {
                ExchangeName = "dev-loyalty-points",
                HostName = "localhost"
            }), new Logger<RabbitMQEventPublisher>(new SerilogLoggerFactory()));
        }

        public async Task AddLoyaltyPoints(string customerIdentifier, string orderIdentifier, decimal orderValue)
        {
            await this._eventPublisher.Publish(new OrderCompletedIntegrationEventV1()
            {
                CustomerIdentifier = customerIdentifier,
                OrderIdentifier = orderIdentifier,
                OrderValue = orderValue
            });

            // Delay to allow for message processing
            await Task.Delay(TimeSpan.FromSeconds(2));
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