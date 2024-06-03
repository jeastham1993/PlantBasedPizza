using System.Text.Json.Serialization;

namespace PlantBasedPizza.OrderManager.Core.AddItemToOrder
{
    public class AddItemToOrderCommand
    {
        [JsonIgnore]
        public string CustomerIdentifier { get; set; } = "";
        
        [JsonIgnore]
        public string OrderIdentifier { get; set; } = "";
        
        public string RecipeIdentifier { get; init; } = "";
        
        public int Quantity { get; init; }
    }
}