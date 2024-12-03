using System.Text.Json.Serialization;

namespace PlantBasedPizza.Orders.IntegrationTest.ViewModels
{
    public class Order
    {
        [JsonPropertyName("orderIdentifier")]
        public string OrderIdentifier { get; set; }
        
        public string OrderNumber { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime? OrderSubmittedOn { get; set; }

        public DateTime? OrderCompletedOn { get; set; }

        [JsonPropertyName("items")]
        public List<OrderItem> Items { get; set; }

        [JsonPropertyName("history")]
        public List<OrderHistory> History { get; set; }

        public int OrderType { get; set; }
        
        [JsonPropertyName("customerIdentifier")]

        public string CustomerIdentifier { get; set; }

        public decimal TotalPrice { get; set; }
        
        public bool AwaitingCollection { get; set; }
    }
}