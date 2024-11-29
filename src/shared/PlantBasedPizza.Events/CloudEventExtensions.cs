using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace PlantBasedPizza.Events;

public static class CloudEventExtensions
{
    public static string ExtractEventId(this HttpContext httpContext)
    {
        foreach (var header in httpContext.Request.Headers)
        {
            Console.WriteLine($"{header.Key}: {header.Value}");    
        }
        
        var cloudEventId = extractValueFromHeader(httpContext, "Cloudevent.id");
        if (!string.IsNullOrEmpty(cloudEventId))
            Activity.Current?.AddTag("messaging.message.id", cloudEventId);

        var cloudEventType = extractValueFromHeader(httpContext, "Cloudevent.type");
        if (!string.IsNullOrEmpty(cloudEventType))
        {
            Activity.Current?.AddTag("messaging.message.type", cloudEventType);
            var eventTypeSplit = cloudEventType.Split('.');
            
            if (eventTypeSplit.Length == 3)
            {
                Activity.Current?.AddTag("messaging.message.version", eventTypeSplit[2]);
            }
        }
            
        Activity.Current?.AddTag("messaging.operation.type", "process");

        return cloudEventId;
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