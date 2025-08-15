using System.Text.Json.Serialization;
using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.OrderManager.Core.CreatePickupOrder
{
    public class CreatePickupOrderCommand
    {
        [JsonPropertyName("orderIdentifier")]
        public string OrderIdentifier { get; init; } = "";

        [JsonPropertyName("customerIdentifier")]
        public string CustomerIdentifier { get; init; } = "";

        [JsonPropertyName("orderType")]
        public OrderType OrderType => OrderType.Pickup;
    }
}