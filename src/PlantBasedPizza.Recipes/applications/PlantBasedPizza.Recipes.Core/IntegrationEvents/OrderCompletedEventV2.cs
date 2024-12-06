namespace PlantBasedPizza.Recipes.Core.IntegrationEvents;

public class OrderCompletedEventV2
{
    public string OrderIdentifier { get; set; }
    
    public Dictionary<string, int>? OrderItems { get; set; }
}