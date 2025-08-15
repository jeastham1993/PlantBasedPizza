using System.Text.Json.Serialization;

namespace PlantBasedPizza.OrderManager.Core.CreatePickupOrder
{
    public class CreatePickupOrderCommand
    {
        [JsonPropertyName("customerIdentifier")]
        public string CustomerIdentifier { get; init; } = "";

        [JsonPropertyName("orderType")]
        public OrderType OrderType => OrderType.Pickup;
    }
}