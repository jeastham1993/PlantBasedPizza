namespace PlantBasedPizza.Kitchen.Api.ViewModel;

public class QueuedMessage
{
    public KitchenRequest Payload { get; set; }
    
    public string CorrelationId { get; set; }
}