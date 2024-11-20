using System.Text.Json.Serialization;

namespace PlantBasedPizza.OrderManager.Core.Entities
{
    public class OrderHistory
    {
        [JsonConstructor]
        private OrderHistory()
        {
            this.Description = "";
        }
        
        public OrderHistory(string description, DateTime historyDate)
        {
            this.Description = description;
            this.HistoryDate = historyDate;
        }
        
        [JsonPropertyName("description")]
        public string Description { get; private set; }
        
        [JsonPropertyName("historyDate")]
        public DateTime HistoryDate { get; private set; }
    }
}