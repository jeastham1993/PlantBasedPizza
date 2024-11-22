using System.Text.Json.Serialization;

namespace PlantBasedPizza.E2ETests.ViewModels
{
    public class Order
    {
        [JsonPropertyName("orderIdentifier")]
        public string OrderIdentifier { get; set; }
        
        [JsonPropertyName("orderNumber")]
        public string OrderNumber { get; set; }
        
        [JsonPropertyName("orderDate")]
        public DateTime OrderDate { get; set; }

        [JsonPropertyName("orderSubmittedOn")]
        public DateTime? OrderSubmittedOn { get; set; }

        [JsonPropertyName("orderCompletedOn")]
        public DateTime? OrderCompletedOn { get; set; }

        [JsonPropertyName("items")]
        public List<OrderItem> Items { get; set; }

        [JsonPropertyName("history")]
        public List<OrderHistory> History { get; set; }
        
        public int OrderType { get; set; }

        public string CustomerIdentifier { get; set; }

        public decimal TotalPrice { get; set; }
        
        [JsonPropertyName("awaitingCollection")]
        public bool AwaitingCollection { get; set; }
    }
}