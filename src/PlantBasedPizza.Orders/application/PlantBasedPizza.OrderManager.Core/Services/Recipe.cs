using System.Text.Json.Serialization;

namespace PlantBasedPizza.OrderManager.Core.Services
{
    public class Recipe
    {
        [JsonPropertyName("itemName")]
        public string ItemName { get; init; } = "";

        [JsonPropertyName("price")]
        public decimal Price { get; init; }
    }
}