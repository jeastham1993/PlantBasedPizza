using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PlantBasedPizza.Events;
using PlantBasedPizza.LoyaltyPoints.Shared.Core;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace PlantBasedPizza.LoyaltyPoints.QueueWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly RabbitMqSettings _settings;
    private readonly AddLoyaltyPointsCommandHandler _handler;

    public Worker(ILogger<Worker> logger, IOptions<RabbitMqSettings> settings, AddLoyaltyPointsCommandHandler handler)
    {
        _logger = logger;
        _handler = handler;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connectionRetry = 3;

        IConnection? connection = null;

        while (connectionRetry > 0)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = _settings.HostName
                };

                connection = factory.CreateConnection();
                break;
            }
            catch (BrokerUnreachableException e)
            {
                connectionRetry--;
                this._logger.LogError($"Broker unreachable, retrying {connectionRetry} more time(s)", e);
                
                await Task.Delay(TimeSpan.FromSeconds(2));
            }   
        }
        
        var channel = connection.CreateModel();

        var queue = channel.QueueDeclare("loyaltypoints-ordercompleted-worker").QueueName;
        
        channel.QueueBind(queue, exchange: _settings.ExchangeName, routingKey: "order.orderCompleted.v1");
        
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var evt = JsonSerializer.Deserialize<OrderCompletedEvent>(message);

            await this._handler.Handle(new AddLoyaltyPointsCommand()
            {
                CustomerIdentifier = evt.CustomerIdentifier,
                OrderValue = evt.OrderValue,
                OrderIdentifier = evt.OrderIdentifier
            });
        };
        
        while (!stoppingToken.IsCancellationRequested)
        {
            channel.BasicConsume(queue,
                autoAck: true,
                consumer: consumer);

            await Task.Delay(1000, stoppingToken);
        }
    }
}