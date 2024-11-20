using System.Diagnostics;
using System.Text.Json;
using Dapr.Client;
using Microsoft.Extensions.Logging;

namespace PlantBasedPizza.Events;

public class DaprEventPublisher : IEventPublisher
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<DaprEventPublisher> _logger;

    public DaprEventPublisher(DaprClient daprClient, ILogger<DaprEventPublisher> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
    }

    public async Task Publish(IntegrationEvent evt)
    {
        var eventId = Guid.NewGuid()
            .ToString();

        Activity.Current?.AddTag("messaging.eventId", eventId);
        Activity.Current?.AddTag("messaging.eventType", evt.EventName);
        Activity.Current?.AddTag("messaging.eventVersion", evt.EventVersion);
        Activity.Current?.AddTag("messagaing.eventSource", evt.Source);
        
        var jsonString = JsonSerializer.Serialize(evt as object);
        
        _logger.LogInformation($"Publishing '{evt.EventName}.{evt.EventVersion}'");
        _logger.LogInformation(jsonString);
        
        await _daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", jsonString);
    }
}