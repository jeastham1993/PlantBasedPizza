namespace PlantBasedPizza.OrderManager.Core.OrderSubmitted;

public class SubmitOrderCommand
{
    public string OrderIdentifier { get; set; } = "";
    
    public string CustomerIdentifier { get; set; } = "";
}