using System.Text.Json.Serialization;

namespace PlantBasedPizza.Recipes.Core.Commands
{
    public record CreateRecipeCommand
    {
        [JsonPropertyName("recipeIdentifier")]
        public string RecipeIdentifier { get; init; } = "";
        
        
        [JsonPropertyName("category")]
        public string Category { get; init; } = "";

        [JsonPropertyName("name")]
        public string Name { get; init; } = "";

        [JsonPropertyName("price")]
        public decimal Price { get; init; }

        [JsonPropertyName("ingredients")]
        public List<CreateRecipeCommandItem> Ingredients { get; init; } = new();
    }

    public record CreateRecipeCommandItem
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = "";

        [JsonPropertyName("quantity")]
        public int Quantity { get; init; }
    }
}