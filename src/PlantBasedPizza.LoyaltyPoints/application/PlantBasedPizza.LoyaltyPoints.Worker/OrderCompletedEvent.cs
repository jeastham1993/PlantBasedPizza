namespace PlantBasedPizza.LoyaltyPoints.Worker;

public record OrderCompletedEvent
{
    public string OrderIdentifier { get; set; }
    
    public string CustomerIdentifier { get; set; }
    
    public decimal OrderValue { get; set; }
}