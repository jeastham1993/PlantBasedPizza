namespace PlantBasedPizza.LoyaltyPoints.QueueWorker;

public record OrderCompletedEvent
{
    public string OrderIdentifier { get; set; }
    
    public string CustomerIdentifier { get; set; }
    
    public decimal OrderValue { get; set; }
}