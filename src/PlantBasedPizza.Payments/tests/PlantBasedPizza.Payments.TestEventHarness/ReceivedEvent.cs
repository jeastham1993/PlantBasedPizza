namespace PlantBasedPizza.Payments.TestEventHarness;

public record ReceivedEvent
{
    public string EntityId { get; set; }
    
    public string EventName { get; set; }
}