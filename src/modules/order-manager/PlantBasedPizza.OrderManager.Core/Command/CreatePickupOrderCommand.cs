using PlantBasedPizza.OrderManager.Core.Entites;

namespace PlantBasedPizza.OrderManager.Core.Command
{
    public class CreatePickupOrderCommand
    {
        public string OrderIdentifier { get; set; }
        
        public string CustomerIdentifier { get; set; }

        public OrderType OrderType => OrderType.PICKUP;
    }
}