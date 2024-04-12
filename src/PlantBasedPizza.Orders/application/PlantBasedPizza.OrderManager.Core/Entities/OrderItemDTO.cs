using System.Text.Json.Serialization;

namespace PlantBasedPizza.OrderManager.Core.Entities;

public class OrderItemDto
{
    [JsonPropertyName("recipeIdentifier")]
    public string RecipeIdentifier { get; set; } = "";
        
    [JsonPropertyName("itemName")]
    public string ItemName { get; set; } = "";
        
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
        
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}