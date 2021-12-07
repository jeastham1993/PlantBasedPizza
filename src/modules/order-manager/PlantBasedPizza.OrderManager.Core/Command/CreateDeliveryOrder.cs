using PlantBasedPizza.OrderManager.Core.Entites;

namespace PlantBasedPizza.OrderManager.Core.Command
{
    public class CreateDeliveryOrder
    {
        public string OrderIdentifier { get; set; }
        
        public string CustomerIdentifier { get; set; }

        public OrderType OrderType => OrderType.DELIVERY;
        
        public string AddressLine1 { get; init; }
        
        public string AddressLine2 { get; init; }
        
        public string AddressLine3 { get; init; }
        
        public string AddressLine4 { get; init; }
        
        public string AddressLine5 { get; init; }
        
        public string Postcode { get; init; }
    }
}