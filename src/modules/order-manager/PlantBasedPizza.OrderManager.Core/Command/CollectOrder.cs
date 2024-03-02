using System.Text.Json.Serialization;

namespace PlantBasedPizza.OrderManager.Core.Command
{
    public class CollectOrderRequest
    {
        [JsonPropertyName("OrderIdentifier")]
        public string OrderIdentifier { get; init; } = "";
    }
}