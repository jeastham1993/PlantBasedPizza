namespace PlantBasedPizza.Orders.IntegrationTest.ViewModels
{
    public class OrderItem
    {
        public string RecipeIdentifier { get; set; }
        
        public string ItemName { get; set; }
        
        public int Quantity { get; set; }
        
        public decimal Price { get; set; }
    }
}