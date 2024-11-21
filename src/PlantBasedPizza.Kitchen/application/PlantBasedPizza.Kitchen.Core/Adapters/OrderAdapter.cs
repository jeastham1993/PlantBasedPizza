namespace PlantBasedPizza.Kitchen.Core.Adapters
{
    public class OrderAdapter
    {
        public OrderAdapter()
        {
            Items = new List<OrderItemAdapter>();
        }
        
        public List<OrderItemAdapter> Items { get; }
    }
}