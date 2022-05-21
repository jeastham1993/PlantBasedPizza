namespace PlantBasedPizza.Kitchen.Api.ViewModel;

public class KitchenRequest
{
    public KitchenRequest()
    {
        this.OrderNumber = string.Empty;
        this.LocationCode = string.Empty;
    }
    
    public string OrderNumber { get; set; }
    
    public string LocationCode { get; set; }
}