using Newtonsoft.Json;

namespace PlantBasedPizza.OrderManager.Core.Entities
{
    public class OrderItem
    {
        [JsonConstructor]
        private OrderItem()
        {
            this.RecipeIdentifier = "";
            this.ItemName = "";
        }
        
        internal OrderItem(string recipeIdentifier, string itemName, int quantity, decimal price)
        {
            this.RecipeIdentifier = recipeIdentifier;
            this.ItemName = itemName;
            this.Quantity = quantity;
            this.Price = price;
        }
        
        [JsonProperty]
        public string RecipeIdentifier { get; private set; }
        
        [JsonProperty]
        public string ItemName { get; private set; }
        
        [JsonProperty]
        public int Quantity { get; private set; }
        
        [JsonProperty]
        public decimal Price { get; private set; }
    }
}