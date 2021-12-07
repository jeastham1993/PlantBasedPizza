using PlantBasedPizza.OrderManager.Core.Entites;

namespace PlantBasedPizza.OrderManager.Core.Command
{
    public class CollectOrderRequest
    {
        public string OrderIdentifier { get; set; }
    }
}