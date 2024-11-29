using System.Text.Json.Serialization;
using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.OrderManager.Core.CreatePickupOrder
{
    public class CreatePickupOrderCommand
    {
        [JsonIgnore]
        public string CustomerIdentifier { get; set; } = "";

        public static OrderType OrderType => OrderType.Pickup;
    }
}