using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlantBasedPizza.Recipes.Core.Commands
{
    public record CreateRecipeCommand
    {
        [JsonPropertyName("RecipeIdentifier")]
        public string RecipeIdentifier { get; init; } = "";

        [JsonPropertyName("Name")]
        public string Name { get; init; } = "";

        [JsonPropertyName("Price")]
        public decimal Price { get; init; }

        [JsonPropertyName("Ingredients")]
        public List<CreateRecipeCommandItem> Ingredients { get; init; } = new();
    }

    public record CreateRecipeCommandItem
    {
        public string Name { get; init; } = "";

        public int Quantity { get; init; }
    }
}