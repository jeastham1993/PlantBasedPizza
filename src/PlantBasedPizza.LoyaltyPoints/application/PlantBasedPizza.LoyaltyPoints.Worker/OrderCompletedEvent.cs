using PlantBasedPizza.Events;

namespace PlantBasedPizza.LoyaltyPoints.Worker;

public class OrderCompletedEvent : IntegrationEvent
{
    public string OrderIdentifier { get; set; }
    
    public string CustomerIdentifier { get; set; }
    
    public decimal OrderValue { get; set; }
    public override string EventName => "order.orderCompleted";
    public override string EventVersion => "v1";
    public override string Source => "https://orders.plantbasedpizza";
}