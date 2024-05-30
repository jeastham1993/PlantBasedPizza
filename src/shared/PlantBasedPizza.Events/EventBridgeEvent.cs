using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace PlantBasedPizza.Events;

public record EventBridgeEvent
{
    [JsonPropertyName("detail")]
    public JsonObject Detail { get; set; }
    
    [JsonPropertyName("time")]
    public DateTime? Time { get; set; }
};