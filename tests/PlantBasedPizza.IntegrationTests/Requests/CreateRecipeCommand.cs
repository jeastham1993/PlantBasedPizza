using System.Collections.Generic;

namespace PlantBasedPizza.IntegrationTests.Requests
{
    public class CreateRecipeCommand
    {
        public string RecipeIdentifier { get; set; }
        
        public string Name { get; set; }
        
        public decimal Price { get; set; }
        
        public List<CreateRecipeCommandItem> Ingredients { get; set; }
    }

    public record CreateRecipeCommandItem
    {
        public string Name { get; set; }
        
        public int Quantity { get; set; }
    }
}