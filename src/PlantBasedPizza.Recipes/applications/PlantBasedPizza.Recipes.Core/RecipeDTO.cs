using System.Text.Json.Serialization;

namespace PlantBasedPizza.Recipes.Core;

public class RecipeDto
{
    public RecipeDto(Recipe recipe)
    {
        Name = recipe.Name;
        Category = recipe.Category.ToString();
        Price = recipe.Price;
        RecipeIdentifier = recipe.RecipeIdentifier;
        Ingredients = recipe.Ingredients;
    }
    
    [JsonPropertyName("recipeIdentifier")]
    public string RecipeIdentifier { get; set; }
        
    [JsonPropertyName("category")]
    public string Category { get; set; }
        
    [JsonPropertyName("name")]
    public string Name { get; set; }
        
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
    
    public IEnumerable<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
}