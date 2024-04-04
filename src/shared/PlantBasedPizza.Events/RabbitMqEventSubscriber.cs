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
    private readonly ILogger<RabbitMqEventSubscriber> _logger;
    private readonly RabbitMqSettings _settings;

    public RabbitMqEventSubscriber(ILogger<RabbitMqEventSubscriber> logger, IOptions<RabbitMqSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<RetrieveEventConsumerResponse> CreateEventConsumer(string queueName, string eventName)
    {
        var connectionRetry = 3;

        IConnection? connection = null;

        while (connectionRetry > 0)
        {
            try
            {
                this._logger.LogError($"Connecting to '{_settings.HostName}'");
                
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
        channel.ExchangeDeclare(exchange: _settings.ExchangeName, ExchangeType.Topic, durable: true);
        
        var queue = channel.QueueDeclare(queueName).QueueName;
        
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
            QueueTime = (DateTimeOffset.Now - evtWrapper.Time.Value).Milliseconds,
            EventId = evtWrapper.Id
        };
    }
}