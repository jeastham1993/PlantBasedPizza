namespace PlantBasedPizza.Kitchen.Core.Adapters
{
    public class OrderItemAdapter
    {
        public string ItemName { get; init; } = "";
        public string RecipeIdentifier { get; init; } = "";
        public int Quantity { get; init; }
    }
}