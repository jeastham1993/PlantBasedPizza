using System.Text.Json.Serialization;

namespace PlantBasedPizza.OrderManager.Core.CollectOrder
{
    public class CollectOrderRequest
    {
        [JsonPropertyName("OrderIdentifier")]
        public string OrderIdentifier { get; init; } = "";
    }
}