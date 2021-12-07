namespace PlantBasedPizza.OrderManager.Core.Entites
{
    public class OrderItem
    {
        internal OrderItem(string recipeIdentifier, string itemName, int quantity, decimal price)
        {
            this.RecipeIdentifier = recipeIdentifier;
            this.ItemName = itemName;
            this.Quantity = quantity;
            this.Price = price;
        }
        
        public string RecipeIdentifier { get; private set; }
        
        public string ItemName { get; private set; }
        
        public int Quantity { get; private set; }
        
        public decimal Price { get; private set; }
    }
}