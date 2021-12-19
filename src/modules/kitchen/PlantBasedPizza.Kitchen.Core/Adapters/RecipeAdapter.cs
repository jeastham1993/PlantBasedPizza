using Newtonsoft.Json;

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
            this.RecipeIdentifier = recipeIdentifier;
        }
        
        public string RecipeIdentifier { get; set; }
    }
}