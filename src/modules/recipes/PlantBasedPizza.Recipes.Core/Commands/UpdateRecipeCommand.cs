namespace PlantBasedPizza.Recipes.Core.Commands
{
    public class UpdateRecipeCommand
    {
        public string RecipeIdentifier { get; set; }
        
        public string Name { get; set; }
        
        public decimal Price { get; set; }
        
        public List<UpdateRecipeCommandItem> Ingredients { get; set; }
    }

    public record UpdateRecipeCommandItem
    {
        public string Name { get; set; }
        
        public int Quantity { get; set; }
    }
}