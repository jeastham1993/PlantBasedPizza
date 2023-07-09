using System.Text.Json.Serialization;

namespace PlantBasedPizza.Shared.Events;

public record EventBridgeEvent<T>
{
    [JsonPropertyName("detail")]
    public T Detail { get; set; }
}