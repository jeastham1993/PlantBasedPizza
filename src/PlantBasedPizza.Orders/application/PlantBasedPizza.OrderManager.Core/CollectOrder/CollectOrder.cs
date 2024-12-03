using System.Text.Json.Serialization;

namespace PlantBasedPizza.OrderManager.Core.CollectOrder
{
    public record CollectOrderRequest
    {
        [JsonPropertyName("OrderIdentifier")]
        public string OrderIdentifier { get; init; } = "";
    }
}