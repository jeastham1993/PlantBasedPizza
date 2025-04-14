namespace PlantBasedPizza.Recipes.Core.OrderCompletedHandler;

public record OrderCompletedEventV2
{
    public string OrderIdentifier { get; set; } = "";
    
    public string CustomerIdentifier { get; set; } = "";
    
    public OrderValue OrderValue { get; set; }
    
    public Dictionary<string, int> OrderItems { get; set; }
}

public record OrderValue
{
    public decimal Value { get; set; }
    public string Currency { get; set; } = "GBP";
}