using Newtonsoft.Json;

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
        
        [JsonProperty]
        public string Description { get; private set; }
        
        [JsonProperty]
        public DateTime HistoryDate { get; private set; }
    }
}