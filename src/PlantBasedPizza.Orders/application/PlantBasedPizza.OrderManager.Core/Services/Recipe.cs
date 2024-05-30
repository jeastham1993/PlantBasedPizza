using System.Text.Json.Serialization;

namespace PlantBasedPizza.OrderManager.Core.Services
{
    public class Recipe
    {
        [JsonPropertyName("name")]
        public string Name { get; init; }

        [JsonPropertyName("price")]
        public decimal Price { get; init; }

        [JsonPropertyName("recipeIdentifier")]
        public string RecipeIdentifier { get; init; }
    }
}