using PlantBasedPizza.Events;

namespace PlantBasedPizza.LoyaltyPoints.Shared.Core;

public class CustomerLoyaltyPointsUpdated : IntegrationEvent
{
    public override string EventName => "loyalty.customerLoyaltyPointsUpdated";
    public override string EventVersion => "v1";
    public override Uri Source => new("https://loyalty.plantbasedpizza");
    
    public string CustomerIdentifier { get; set; }
    
    public decimal TotalLoyaltyPoints { get; set; }
}