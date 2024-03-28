namespace PlantBasedPizza.E2ETests.Requests
{
    public class AddItemToOrderCommand
    {
        public string OrderIdentifier { get; set; }
        
        public string RecipeIdentifier { get; set; }
        
        public int Quantity { get; set; }
    }
}