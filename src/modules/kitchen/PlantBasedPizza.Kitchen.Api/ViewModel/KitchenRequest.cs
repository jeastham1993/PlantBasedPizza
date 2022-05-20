namespace PlantBasedPizza.Kitchen.Api.ViewModel;

public class KitchenRequest
{
    public KitchenRequest()
    {
        this.OrderNumber = string.Empty;
    }
    
    public string OrderNumber { get; set; }
}