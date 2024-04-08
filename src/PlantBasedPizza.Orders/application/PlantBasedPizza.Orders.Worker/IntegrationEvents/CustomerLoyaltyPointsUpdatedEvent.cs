using PlantBasedPizza.Events;

namespace PlantBasedPizza.Orders.Worker.IntegrationEvents;

public class CustomerLoyaltyPointsUpdatedEvent : IntegrationEvent
{
    public string CustomerIdentifier { get; set; }
    
    public decimal TotalLoyaltyPoints { get; set; }
    public override string EventName => "loyalty.customerLoyaltyPointsUpdated";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("https://orders.test.plantbasedpizza");
}