using System.Diagnostics;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Microsoft.VisualBasic.CompilerServices;

namespace PlantBasedPizza.Events;

public class EventBridgeEventPublisher : IEventPublisher
{
    private readonly AmazonEventBridgeClient _eventBridgeClient;
    private readonly RabbitMqSettings _settings;

    public EventBridgeEventPublisher(AmazonEventBridgeClient eventBridgeClient, RabbitMqSettings settings)
    {
        _eventBridgeClient = eventBridgeClient;
        _settings = settings;
    }

    public async Task Publish(IntegrationEvent evt)
    {
        var eventType = $"{evt.EventName}.{evt.EventVersion}";
        
        var eventId = Guid.NewGuid()
            .ToString();

        Activity.Current?.AddTag("messaging.eventId", eventId);
        Activity.Current?.AddTag("messaging.eventType", evt.EventName);
        Activity.Current?.AddTag("messaging.eventVersion", evt.EventVersion);
        Activity.Current?.AddTag("messaging.eventSource", evt.Source);

        var evtWrapper = new CloudEvent
        {
            Type = eventType,
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

        var evtFormatter = new JsonEventFormatter();

        var json = evtFormatter.ConvertToJsonElement(evtWrapper).ToString();

        await _eventBridgeClient.PutEventsAsync(new PutEventsRequest()
        {
            Entries = new List<PutEventsRequestEntry>(1)
            {
                new()
                {
                    Source = evt.Source.ToString(),
                    EventBusName = _settings.ExchangeName,
                    Detail = json,
                    DetailType = eventType
                }
            }
        });
    }
}