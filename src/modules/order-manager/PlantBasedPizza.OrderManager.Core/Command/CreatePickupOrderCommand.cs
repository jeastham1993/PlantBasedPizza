using System.Text.Json.Serialization;
using PlantBasedPizza.OrderManager.Core.Entites;

namespace PlantBasedPizza.OrderManager.Core.Command
{
    public class CreatePickupOrderCommand
    {
        [JsonPropertyName("OrderIdentifier")]
        public string OrderIdentifier { get; init; } = "";
        
        [JsonPropertyName("CustomerIdentifier")]
        public string CustomerIdentifier { get; init; } = "";

        public OrderType OrderType => OrderType.PICKUP;
    }
}