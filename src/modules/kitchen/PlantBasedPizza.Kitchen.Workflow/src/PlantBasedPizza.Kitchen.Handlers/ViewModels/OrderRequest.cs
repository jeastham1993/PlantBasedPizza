namespace PlantBasedPizza.Kitchen.Handlers.ViewModels;

public class QueuedMessage
{
    public OrderRequest Payload { get; set; }
    
    public string CorrelationId { get; set; }
}

public class OrderRequest
{
    public OrderRequest()
    {
        this.OrderNumber = string.Empty;
    }
    
    public string OrderNumber { get; set; }
}