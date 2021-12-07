namespace PlantBasedPizza.Recipes.Core.Entities
{
    public class Ingredient
    {
        public Ingredient(string name, int quantity)
        {
            this.Name = name;
            this.Quantity = quantity;
        }
        
        public string Name { get; }
        
        public int Quantity { get; }
    }
}