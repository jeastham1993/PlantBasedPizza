using System.Text.Json.Serialization;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Entities;

namespace PlantBasedPizza.Kitchen.Infrastructure.DataTransfer;

public class KitchenRequestDto
{
    public KitchenRequestDto()
    {
        this.KitchenRequestId = "";
        this.OrderIdentifier = "";
        this.ItemsOnOrder = new List<RecipeDto>();
    }

    public KitchenRequestDto(KitchenRequest request)
    {
        this.KitchenRequestId = request.KitchenRequestId;
        this.OrderIdentifier = request.OrderIdentifier;
        this.OrderReceivedOn = request.OrderReceivedOn;
        this.PrepCompleteOn = request.PrepCompleteOn;
        this.BakeCompleteOn = request.BakeCompleteOn;
        this.QualityCheckCompleteOn = request.QualityCheckCompleteOn;
        this.ItemsOnOrder = request.Recipes.Select(recipe => new RecipeDto(recipe)).ToList();
    }

    [JsonPropertyName("kitchenRequestId")]
    public string KitchenRequestId { get; set; }
    
    [JsonPropertyName("orderIdentifier")]
    public string OrderIdentifier { get; set; }
    
    [JsonPropertyName("orderReceivedOn")]
    public DateTime OrderReceivedOn { get; set; }
        
    [JsonPropertyName("prepCompleteOn")]
    public DateTime? PrepCompleteOn { get; set; }
        
    [JsonPropertyName("bakeCompleteOn")]
    public DateTime? BakeCompleteOn { get; set; }
        
    [JsonPropertyName("qualityCheckCompleteOn")]
    public DateTime? QualityCheckCompleteOn { get; set; }
    
    [JsonPropertyName("itemsOnOrder")]
    public List<RecipeDto> ItemsOnOrder { get; set; }
}

public class RecipeDto
{
    public RecipeDto(RecipeAdapter adapter)
    {
        this.RecipeIdentifier = adapter.RecipeIdentifier;
        this.Ingredients = adapter.Ingredients.Select(ingredient => new RecipeItemDto()
        {
            Name = ingredient.Name,
            Quantity = ingredient.Quantity
        }).ToList();

    }

    [JsonPropertyName("recipeIdentifier")]
    public string RecipeIdentifier { get; set; } = "";
        
    [JsonPropertyName("ingredients")]
    public List<RecipeItemDto> Ingredients { get; set; }
}

public record RecipeItemDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
        
    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }
}