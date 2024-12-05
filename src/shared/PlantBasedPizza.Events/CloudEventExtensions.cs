using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace PlantBasedPizza.Events;

public record EventData(string EventId, string EventType, string TraceParent);

public static class CloudEventExtensions
{
    public static string ExtractEventId(this HttpContext httpContext)
    {
        var cloudEventId = extractValueFromHeader(httpContext, EventConstants.EVENT_ID_HEADER_KEY);
        if (!string.IsNullOrEmpty(cloudEventId))
            Activity.Current?.AddTag("messaging.message.id", cloudEventId);

        var cloudEventType = extractValueFromHeader(httpContext, EventConstants.EVENT_TYPE_HEADER_KEY);
        if (!string.IsNullOrEmpty(cloudEventType))
        {
            Activity.Current?.AddTag("messaging.message.type", cloudEventType);
            var eventTypeSplit = cloudEventType.Split('.');

            if (eventTypeSplit.Length == 3) Activity.Current?.AddTag("messaging.message.version", eventTypeSplit[2]);
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
}