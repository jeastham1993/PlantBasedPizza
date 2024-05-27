using System.Diagnostics;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Microsoft.Extensions.Options;

namespace PlantBasedPizza.Events;

public class EventBridgeEventPublisher : IEventPublisher
{
    private readonly AmazonEventBridgeClient _eventBridgeClient;
    private readonly EventBridgeSettings _settings;

    public EventBridgeEventPublisher(AmazonEventBridgeClient eventBridgeClient, IOptions<EventBridgeSettings> settings)
    {
        _eventBridgeClient = eventBridgeClient;
        _settings = settings.Value;
    }

    public async Task Publish(IntegrationEvent evt)
    {
        var eventType = $"{evt.EventName}.{evt.EventVersion}";

        using var publishActivity = Activity.Current?.Source.StartActivity($"Publishing{eventType}");
        
        var eventId = Guid.NewGuid()
            .ToString();

        publishActivity.AddTag("messaging.eventId", eventId);
        publishActivity.AddTag("messaging.eventType", eventType);
        publishActivity.AddTag("messaging.eventName", evt.EventName);
        publishActivity.AddTag("messaging.eventVersion", evt.EventVersion);
        publishActivity.AddTag("messaging.eventSource", evt.Source);
        publishActivity.AddTag("messaging.busName", _settings.BusName);

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

        var publishResponse = await _eventBridgeClient.PutEventsAsync(new PutEventsRequest()
        {
            Entries = new List<PutEventsRequestEntry>(1)
            {
                new()
                {
                    Source = evt.Source.ToString(),
                    EventBusName = _settings.BusName,
                    Detail = json,
                    DetailType = eventType
                }
            }
        });

        publishActivity.AddTag("messaging.failures", publishResponse.FailedEntryCount);
    }
}