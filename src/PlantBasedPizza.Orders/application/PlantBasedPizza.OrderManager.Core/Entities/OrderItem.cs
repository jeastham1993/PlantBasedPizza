using System.Text.Json.Serialization;

namespace PlantBasedPizza.OrderManager.Core.Entities
{
    public class OrderItem
    {
        [JsonConstructor]
        private OrderItem()
        {
            RecipeIdentifier = "";
            ItemName = "";
        }
        
        internal OrderItem(string recipeIdentifier, string itemName, int quantity, decimal price)
        {
            RecipeIdentifier = recipeIdentifier;
            ItemName = itemName;
            Quantity = quantity;
            Price = price;
        }
        
        [JsonPropertyName("recipeIdentifier")]
        public string RecipeIdentifier { get; private set; }
        
        [JsonPropertyName("itemName")]
        public string ItemName { get; private set; }
        
        [JsonPropertyName("quantity")]
        public int Quantity { get; private set; }
        
        [JsonPropertyName("price")]
        public decimal Price { get; private set; }
    }
}