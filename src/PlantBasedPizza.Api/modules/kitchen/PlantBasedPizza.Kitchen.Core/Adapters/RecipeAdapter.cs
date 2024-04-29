using System.Text.Json.Serialization;

namespace PlantBasedPizza.Kitchen.Core.Adapters
{
    public class RecipeAdapter
    {
        [JsonConstructor]
        private RecipeAdapter()
        {
            this.Ingredients = new List<RecipeItemAdapter>();
        }
        
        public RecipeAdapter(string recipeIdentifier)
        {
            this.RecipeIdentifier = recipeIdentifier;
            this.Ingredients = new List<RecipeItemAdapter>();
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