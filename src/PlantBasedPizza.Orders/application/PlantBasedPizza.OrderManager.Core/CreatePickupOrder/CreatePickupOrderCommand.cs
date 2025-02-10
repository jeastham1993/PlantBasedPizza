using System.Text.Json.Serialization;

namespace PlantBasedPizza.OrderManager.Core.CreatePickupOrder
{
    public class CreatePickupOrderCommand
    {
        [JsonIgnore]
        public string CustomerIdentifier { get; set; } = "";

        public static OrderType OrderType => OrderType.Pickup;
    }
}