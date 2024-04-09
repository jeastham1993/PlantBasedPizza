using System.Text;
using System.Text.Json;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlantBasedPizza.Events;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;
using Serilog.Extensions.Logging;

namespace PlantBasedPizza.Orders.IntegrationTest.Drivers;

public class OrdersTestDriver
    {
        private readonly IEventPublisher _eventPublisher;

        public OrdersTestDriver()
        {
            _eventPublisher = new RabbitMQEventPublisher(new OptionsWrapper<RabbitMqSettings>(new RabbitMqSettings()
            {
                ExchangeName = "dev.plantbasedpizza",
                HostName = "localhost"
            }), new Logger<RabbitMQEventPublisher>(new SerilogLoggerFactory()),  new RabbitMQConnection("localhost"));
        }

        public async Task SimulateLoyaltyPointsUpdatedEvent(string customerIdentifier, decimal totalPoints)
        {
            await this._eventPublisher.Publish(new CustomerLoyaltyPointsUpdatedEvent()
            {
                CustomerIdentifier = customerIdentifier,
                TotalLoyaltyPoints = totalPoints
            });

            // Delay to allow for message processing
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }