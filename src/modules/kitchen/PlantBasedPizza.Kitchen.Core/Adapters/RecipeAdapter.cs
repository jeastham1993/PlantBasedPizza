namespace PlantBasedPizza.Kitchen.Core.Adapters
{
    public class RecipeAdapter
    {
        public RecipeAdapter(string recipeIdentifier)
        {
            this.RecipeIdentifier = recipeIdentifier;
        }
        
        public string RecipeIdentifier { get; set; }
    }
}