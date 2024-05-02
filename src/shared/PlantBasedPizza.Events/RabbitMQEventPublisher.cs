using System.Diagnostics;
using System.Text;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using JsonEventFormatter = CloudNative.CloudEvents.SystemTextJson.JsonEventFormatter;

namespace PlantBasedPizza.Events;

public class RabbitMQEventPublisher : IEventPublisher
{
    private readonly RabbitMqSettings _rabbitMqSettings;
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMQEventPublisher> _logger;
    
    public RabbitMQEventPublisher(IOptions<RabbitMqSettings> settings, ILogger<RabbitMQEventPublisher> logger, RabbitMQConnection connection)
    {
        _logger = logger;
        _connection = connection.Connection;
        _rabbitMqSettings = settings.Value;
    }
    
    public Task Publish(IntegrationEvent evt)
    {
        var channel = _connection.CreateModel();

        channel.ExchangeDeclare(exchange: _rabbitMqSettings.ExchangeName, ExchangeType.Topic, durable: true);

        var queueName = $"{evt.EventName}.{evt.EventVersion}";

        channel.QueueDeclare(queueName, exclusive: false, durable: true);

        var eventId = Guid.NewGuid()
            .ToString();

        Activity.Current?.AddTag("messaging.eventId", eventId);
        Activity.Current?.AddTag("messaging.eventType", evt.EventName);
        Activity.Current?.AddTag("messaging.eventVersion", evt.EventVersion);
        Activity.Current?.AddTag("messagaing.eventSource", evt.Source);

        var evtWrapper = new CloudEvent
        {
            Type = queueName,
            Source = evt.Source,
            Time = DateTimeOffset.Now,
            DataContentType = "application/json",
            Id = eventId,
            Data = evt,
        };

        if (!string.IsNullOrEmpty(Activity.Current?.Id))
        {
            evtWrapper.SetAttributeFromString("traceparent", Activity.Current?.Id!);   
        }
        
        this._logger.LogInformation("Publishing event {EventId} {EventVersion} with traceId {TraceId}", evtWrapper.Id, evt.EventVersion, Activity.Current?.Id);

        var evtFormatter = new JsonEventFormatter();

        var json = evtFormatter.ConvertToJsonElement(evtWrapper).ToString();
        
        this._logger.LogInformation(json);
        
        var body = Encoding.UTF8.GetBytes(json);
        
        this._logger.LogInformation($"Publishing '{queueName}' to '{_rabbitMqSettings.ExchangeName}'");
        
        //put the data on to the product queue
        channel.BasicPublish(exchange: _rabbitMqSettings.ExchangeName, routingKey: $"{evt.EventName}.{evt.EventVersion}", body: body);

        return Task.CompletedTask;
    }
}