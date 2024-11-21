using System.Text.Json.Serialization;

namespace PlantBasedPizza.Kitchen.Core.Adapters
{
    public class RecipeAdapter
    {
        [JsonConstructor]
        private RecipeAdapter()
        {
        }
        
        public RecipeAdapter(string recipeIdentifier)
        {
            RecipeIdentifier = recipeIdentifier;
        }
        
        [JsonPropertyName("recipeIdentifier")]
        public string RecipeIdentifier { get; set; } = "";
        
        [JsonPropertyName("ingredients")]
        public List<RecipeItemAdapter> Ingredients { get; set; }
    }

    public record RecipeItemAdapter
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("quantity")]
        public decimal Quantity { get; set; }
    }
}