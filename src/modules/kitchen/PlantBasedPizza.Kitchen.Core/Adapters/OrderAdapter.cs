using System.Collections.Generic;

namespace PlantBasedPizza.Kitchen.Core.Adapters
{
    public class OrderAdapter
    {
        public OrderAdapter()
        {
            this.Items = new List<OrderItemAdapter>();
        }
        
        public List<OrderItemAdapter> Items { get; }
    }
}