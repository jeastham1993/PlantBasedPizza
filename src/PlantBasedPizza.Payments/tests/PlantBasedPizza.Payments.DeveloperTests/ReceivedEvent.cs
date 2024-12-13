using System.Text.Json.Serialization;

namespace PlantBasedPizza.Payments.InMemoryTests;

public record ReceivedEvent
{
    [JsonPropertyName("entityId")]
    public string EntityId { get; set; }
    
    [JsonPropertyName("eventName")]
    public string EventName { get; set; }
}