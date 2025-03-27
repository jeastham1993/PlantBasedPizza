using System.Text.Json.Serialization;

namespace PlantBasedPizza.Recipes.Core
{
    public class Ingredient
    {
        [JsonConstructor]
        private Ingredient()
        {
        }
        
        public Ingredient(string name, int quantity)
        {
            Name = name;
            Quantity = quantity;
        }

        public string Name { get; } = "";

        public int Quantity { get; }
    }
}