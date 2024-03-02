namespace PlantBasedPizza.OrderManager.Core.Command
{
    public class AddItemToOrderCommand
    {
        public string OrderIdentifier { get; init; } = "";
        
        public string RecipeIdentifier { get; init; } = "";
        
        public int Quantity { get; init; }
    }
}