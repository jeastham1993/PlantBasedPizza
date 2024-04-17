using System.Text.Json.Serialization;
using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.OrderManager.Core.CreatePickupOrder
{
    public class CreatePickupOrderCommand
    {
        [JsonPropertyName("orderIdentifier")]
        public string OrderIdentifier { get; init; } = "";
        
        [JsonIgnore]
        public string CustomerIdentifier { get; set; } = "";

        public OrderType OrderType => OrderType.Pickup;
    }
}