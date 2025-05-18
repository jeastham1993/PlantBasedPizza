using System.Text.Json.Serialization;

namespace PlantBasedPizza.Recipes.Core.Entities
{
    public class Ingredient
    {
        [JsonConstructor]
        private Ingredient()
        {
        }
        
        public Ingredient(string name, int quantity)
        {
            this.Name = name;
            this.Quantity = quantity;
        }
        
        public int IngredientIdentifier { get; set; }
        
        public string RecipeIdentifier { get; private set; }

        public string Name { get; private set; } = "";

        public int Quantity { get; private set; }
    }
}