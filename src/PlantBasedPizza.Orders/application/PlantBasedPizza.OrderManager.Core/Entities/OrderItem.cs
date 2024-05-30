using System.Text.Json.Serialization;

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
        
        [JsonPropertyName("recipeIdentifier")]
        public string RecipeIdentifier { get; init; }
        
        [JsonPropertyName("itemName")]
        public string ItemName { get; init; }
        
        [JsonPropertyName("quantity")]
        public int Quantity { get; init; }
        
        [JsonPropertyName("price")]
        public decimal Price { get; init; }
    }
}