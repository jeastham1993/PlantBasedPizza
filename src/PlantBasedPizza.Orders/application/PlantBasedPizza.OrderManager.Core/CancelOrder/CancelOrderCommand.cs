using System.Text.Json.Serialization;

namespace PlantBasedPizza.OrderManager.Core.CancelOrder;

public record CancelOrderCommand
{
    [JsonPropertyName("orderIdentifier")]
    public string OrderIdentifier { get; set; }
}