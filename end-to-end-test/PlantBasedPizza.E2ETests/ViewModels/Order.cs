namespace PlantBasedPizza.E2ETests.ViewModels
{
    public class Order
    {
        public string OrderIdentifier { get; set; }
        
        public string OrderNumber { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime? OrderSubmittedOn { get; set; }

        public DateTime? OrderCompletedOn { get; set; }

        public List<OrderItem> Items { get; set; }

        public List<OrderHistory> History { get; set; }

        public int OrderType { get; set; }

        public string CustomerIdentifier { get; set; }

        public decimal TotalPrice { get; set; }
        
        public bool AwaitingCollection { get; set; }
    }
}