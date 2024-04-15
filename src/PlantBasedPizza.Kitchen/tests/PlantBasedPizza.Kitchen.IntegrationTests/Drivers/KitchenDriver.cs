using System.Text;
using System.Text.Json;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.IntegrationTests.ViewModels;
using Serilog.Extensions.Logging;

namespace PlantBasedPizza.Kitchen.IntegrationTests.Drivers;

public class KitchenDriver
{
    private static string BaseUrl = TestConstants.DefaultTestUrl;

        private readonly HttpClient _httpClient;
        private readonly IEventPublisher _eventPublisher;

        public KitchenDriver()
        {
            this._httpClient = new HttpClient();

            _eventPublisher = new RabbitMQEventPublisher(new OptionsWrapper<RabbitMqSettings>(new RabbitMqSettings()
            {
                ExchangeName = "dev.kitchen",
                HostName = "localhost"
            }), new Logger<RabbitMQEventPublisher>(new SerilogLoggerFactory()), new RabbitMQConnection("localhost"));
        }

        public async Task NewOrderSubmitted(string orderIdentifier)
        {
            await this._eventPublisher.Publish(new OrderSubmittedEventV1()
            {
                OrderIdentifier = orderIdentifier,
                Items = new List<OrderSubmittedEventItem>(1)
                {
                    new()
                    {
                        ItemName = "pizza",
                        RecipeIdentifier = "pizza"
                    }
                }
            });

            // Delay to allow for message processing
            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        public async Task<List<KitchenRequestDto>> GetNewOrders()
        {
            var result = await this._httpClient.GetAsync($"{TestConstants.DefaultTestUrl}/kitchen/new");

            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("Failure retrieving new kitchen orders");
            }

            return JsonSerializer.Deserialize<List<KitchenRequestDto>>(await result.Content.ReadAsStringAsync());
        }
}