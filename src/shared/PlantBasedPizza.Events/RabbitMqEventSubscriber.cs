using System.Diagnostics;
using System.Net.Mime;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace PlantBasedPizza.Events;

public class RabbitMqEventSubscriber
{
    private readonly RabbitMQConnection _connection;
    private readonly RabbitMqSettings _settings;

    public RabbitMqEventSubscriber(RabbitMQConnection connection, ILogger<RabbitMqEventSubscriber> logger, IOptions<RabbitMqSettings> settings)
    {
        _connection = connection;
        _settings = settings.Value;
    }

    public RetrieveEventConsumerResponse CreateEventConsumer(string queueName, string eventName, int deliveryLimit = 3)
    {
        var channel = this._connection.Connection.CreateModel();
        channel.ExchangeDeclare(exchange: _settings.ExchangeName, ExchangeType.Topic, durable: true);
        channel.ExchangeDeclare(exchange: $"{_settings.ExchangeName}-dlq", ExchangeType.Direct, durable: true);

        var dlq = channel.QueueDeclare($"{queueName}-dlq", durable: true, autoDelete: false, exclusive: false,
            arguments: new Dictionary<string, object>(1)
            {
                { "x-queue-type", "quorum" },
            });
        
        var queue = channel.QueueDeclare(queueName, durable: true, autoDelete: false, exclusive: false, arguments: new Dictionary<string, object>(1)
        {
            {"x-queue-type", "quorum"},
            {"x-delivery-limit", deliveryLimit},
            {"x-dead-letter-exchange", $"{_settings.ExchangeName}-dlq"},
            {"x-dead-letter-routing-key", dlq.QueueName}
        }).QueueName;
        
        channel.QueueBind(dlq, exchange: $"{_settings.ExchangeName}-dlq", routingKey: dlq.QueueName);
        channel.QueueBind(queue, exchange: _settings.ExchangeName, routingKey: eventName);
        
        return new RetrieveEventConsumerResponse(channel);
    }
    
    public async Task<ParseEventResponse<T>> ParseEventFrom<T>(byte[] messageBody) where T : class {
        var formatter = new JsonEventFormatter<T>();
        var evtWrapper = await formatter.DecodeStructuredModeMessageAsync(new MemoryStream(messageBody), new ContentType("application/json"), new List<CloudEventAttribute>(1)
        {
            CloudEventAttribute.CreateExtension("traceparent", CloudEventAttributeType.String)
        });
        
        var traceParent = "";
            
        foreach (var (attribute, value) in evtWrapper.GetPopulatedAttributes())
        {
            if (attribute.Name == "traceparent")
            {
                traceParent = value.ToString();
            }
        }

        var evtData = evtWrapper.Data as T;

        return new ParseEventResponse<T>()
        {
            EventData = evtData,
            TraceParent = traceParent,
            QueueTime = (DateTimeOffset.Now - evtWrapper.Time!.Value).Milliseconds,
            EventId = evtWrapper.Id!
        };
    }
}