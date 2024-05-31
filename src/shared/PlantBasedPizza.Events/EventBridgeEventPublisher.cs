using System.Diagnostics;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Datadog.Trace;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PlantBasedPizza.Events;

public class EventBridgeEventPublisher : IEventPublisher
{
    private readonly AmazonEventBridgeClient _eventBridgeClient;
    private readonly EventBridgeSettings _settings;
    private readonly ILogger<EventBridgeEventPublisher> _logger;

    public EventBridgeEventPublisher(AmazonEventBridgeClient eventBridgeClient, IOptions<EventBridgeSettings> settings)
    {
        _eventBridgeClient = eventBridgeClient;
        _settings = settings.Value;
    }

    public async Task Publish(IntegrationEvent evt)
    {
        var eventType = $"{evt.EventName}.{evt.EventVersion}";
        
        var eventId = Guid.NewGuid()
            .ToString();

        Tracer.Instance.ActiveScope?.Span.SetTag("messaging.eventId", eventId);
        Tracer.Instance.ActiveScope?.Span.SetTag("messaging.eventType", eventType);
        Tracer.Instance.ActiveScope?.Span.SetTag("messaging.eventName", evt.EventName);
        Tracer.Instance.ActiveScope?.Span.SetTag("messaging.eventVersion", evt.EventVersion);
        Tracer.Instance.ActiveScope?.Span.SetTag("messaging.eventSource", evt.Source.ToString());
        Tracer.Instance.ActiveScope?.Span.SetTag("messaging.busName", _settings.BusName);

        var evtWrapper = new CloudEvent
        {
            Type = eventType,
            Source = evt.Source,
            Time = DateTimeOffset.Now,
            DataContentType = "application/json",
            Id = eventId,
            Data = evt,
        };
        
        if (Tracer.Instance.ActiveScope?.Span != null)
        {
            evtWrapper.SetAttributeFromString("ddtraceid", Tracer.Instance.ActiveScope.Span.TraceId.ToString());
            evtWrapper.SetAttributeFromString("ddspanid", Tracer.Instance.ActiveScope.Span.SpanId.ToString());
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

        Tracer.Instance.ActiveScope?.Span.SetTag("messaging.failedEvents", publishResponse.FailedEntryCount);

        if (publishResponse.FailedEntryCount > 0)
        {
            foreach (var failedEvent in publishResponse.Entries.Where(p => string.IsNullOrEmpty(p.EventId)))
            {
                this._logger.LogError("Event publish failed with {ErrorCode} and {ErrorMessage}", failedEvent.ErrorCode, failedEvent.ErrorMessage);
            }
        }
    }
}