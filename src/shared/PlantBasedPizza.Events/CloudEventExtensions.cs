using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using OpenTelemetry.Trace;

namespace PlantBasedPizza.Events;

public record EventData(string EventId, string EventType, string TraceParent);

public enum EventType
{
    PUBLIC,
    PRIVATE
}

public record SemanticConventions(
    EventType EventType,
    string EventName,
    string EventId,
    string MessagingSystem,
    string DestinationName,
    string ClientId,
    string ConversationId = null);

public static class CloudEventExtensions
{
    public static string ExtractEventId(this HttpContext httpContext)
    {
        var cloudEventId = extractValueFromHeader(httpContext, EventConstants.EVENT_ID_HEADER_KEY);

        var cloudEventTime = extractValueFromHeader(httpContext, EventConstants.EVENT_TIME_HEADER_KEY);
        if (!string.IsNullOrEmpty(cloudEventTime))
        {
            var publishTime = DateTime.Parse(cloudEventTime);

            var messageAge = DateTime.UtcNow - publishTime;

            Activity.Current?.AddTag("messaging.message.publish_time",
                publishTime.ToString(CultureInfo.InvariantCulture));
            Activity.Current?.AddTag("messaging.message.age",
                messageAge.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
        }

        Activity.Current?.AddTag("messaging.operation.type", "process");

        return cloudEventId;
    }

    public static EventData ExtractEventData(this HttpContext httpContext)
    {
        var cloudEventId = extractValueFromHeader(httpContext, EventConstants.EVENT_ID_HEADER_KEY);
        if (!string.IsNullOrEmpty(cloudEventId))
            Activity.Current?.AddTag("messaging.message.id", cloudEventId);

        var cloudEventType = extractValueFromHeader(httpContext, EventConstants.EVENT_TYPE_HEADER_KEY);

        if (!string.IsNullOrEmpty(cloudEventType))
        {
            Activity.Current?.AddTag("messaging.message.type", cloudEventType);
            var eventTypeSplit = cloudEventType.Split('.');

            if (eventTypeSplit.Length == 3)
            {
                Activity.Current?.AddTag("messaging.message.version", eventTypeSplit[2]);
            }
        }

        var cloudEventTime = extractValueFromHeader(httpContext, EventConstants.EVENT_TIME_HEADER_KEY);
        if (!string.IsNullOrEmpty(cloudEventTime))
        {
            var publishTime = DateTime.Parse(cloudEventTime);

            var messageAge = DateTime.UtcNow - publishTime;

            Activity.Current?.AddTag("messaging.message.publish_time",
                publishTime.ToString(CultureInfo.InvariantCulture));
            Activity.Current?.AddTag("messaging.message.age",
                messageAge.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
        }
        
        var traceParent = extractValueFromHeader(httpContext, "traceparent") ?? "";

        Activity.Current?.AddTag("messaging.operation.type", "process");

        return new EventData(cloudEventId, cloudEventType, traceParent);
    }

    private static string extractValueFromHeader(HttpContext httpContext, string headerName)
    {
        var headerValue =
            httpContext.Request.Headers.FirstOrDefault(
                h => h.Key.Equals(headerName, StringComparison.OrdinalIgnoreCase));
        var cloudEventIdValue = headerValue.Value.FirstOrDefault();
        if (!headerValue.Value.Any() || string.IsNullOrEmpty(cloudEventIdValue)) return "";

        return cloudEventIdValue;
    }

    public static Activity? StartActivityWithSemanticConventions(this ActivitySource source, SemanticConventions semanticConventions)
    {
        var activity = source.StartActivity($"publish {semanticConventions.EventName}");

        if (activity is null)
        {
            return activity;
        }
        
        activity.AddTag("messaging.message.eventType", semanticConventions.EventType.ToString());
        activity.AddTag("messaging.message.type", semanticConventions.EventName);
        activity.AddTag("messaging.message.id", semanticConventions.EventId);
        activity.AddTag("messaging.operation.type", "publish");
        activity.AddTag("messaging.operation.name", "publish");
        activity.AddTag("messaging.system", semanticConventions.MessagingSystem);
        activity.AddTag("messaging.batch.message.count", 1);
        activity.AddTag("messaging.destination.name", semanticConventions.DestinationName);
        activity.AddTag("messaging.client.id", semanticConventions.ClientId);
        
        activity.AddTag("messaging.system", "dapr.pubsub");

        var domain = Environment.GetEnvironmentVariable("DOMAIN");

        if (!string.IsNullOrEmpty(domain))
        {
            activity.AddTag("domain", domain);
            activity.AddTag("messaging.message.domain", domain);
        }
        
        if (!string.IsNullOrEmpty(semanticConventions.ConversationId))
        {
            activity.AddTag("messaging.message.conversation_id", semanticConventions.ConversationId);
        }
        
        if (!string.IsNullOrEmpty(semanticConventions.EventName))
        {
            var eventTypeSplit = semanticConventions.EventName.Split('.');

            if (eventTypeSplit.Length == 3) Activity.Current?.AddTag("messaging.message.version", eventTypeSplit[2]);
        }
        
        return activity;
    }
    
    public static Activity? StartActivityWithProcessSemanticConventions(this ActivitySource source, SemanticConventions semanticConventions)
    {
        var activity = source.StartActivity($"process {semanticConventions.EventName}");

        if (activity is null)
        {
            return activity;
        }
        
        activity.AddTag("messaging.message.eventType", semanticConventions.EventType.ToString());
        activity.AddTag("messaging.message.type", semanticConventions.EventName);
        activity.AddTag("messaging.message.id", semanticConventions.EventId);
        activity.AddTag("messaging.operation.type", "process");
        activity.AddTag("messaging.operation.name", "process");
        activity.AddTag("messaging.system", semanticConventions.MessagingSystem);
        activity.AddTag("messaging.batch.message.count", 1);
        activity.AddTag("messaging.destination.name", semanticConventions.DestinationName);
        activity.AddTag("messaging.client.id", semanticConventions.ClientId);
        
        activity.AddTag("messaging.system", "dapr.pubsub");

        var domain = Environment.GetEnvironmentVariable("DOMAIN");

        if (!string.IsNullOrEmpty(domain))
        {
            activity.AddTag("domain", domain);
            activity.AddTag("messaging.message.domain", domain);
        }
        
        if (!string.IsNullOrEmpty(semanticConventions.ConversationId))
        {
            activity.AddTag("messaging.message.conversation_id", semanticConventions.ConversationId);
        }
        
        if (!string.IsNullOrEmpty(semanticConventions.EventName))
        {
            var eventTypeSplit = semanticConventions.EventName.Split('.');

            if (eventTypeSplit.Length == 3) Activity.Current?.AddTag("messaging.message.version", eventTypeSplit[2]);
        }
        
        return activity;
    }
}