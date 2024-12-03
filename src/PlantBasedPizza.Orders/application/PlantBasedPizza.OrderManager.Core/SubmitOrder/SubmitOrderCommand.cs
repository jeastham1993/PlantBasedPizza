namespace PlantBasedPizza.OrderManager.Core.SubmitOrder;

public class SubmitOrderCommand
{
    public string OrderIdentifier { get; set; } = "";
    
    public string CustomerIdentifier { get; set; } = "";
}