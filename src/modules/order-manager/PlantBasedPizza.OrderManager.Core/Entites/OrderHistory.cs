using System;

namespace PlantBasedPizza.OrderManager.Core.Entites
{
    public class OrderHistory
    {
        public OrderHistory(string description, DateTime historyDate)
        {
            this.Description = description;
            this.HistoryDate = historyDate;
        }
        
        public string Description { get; private set; }
        
        public DateTime HistoryDate { get; private set; }
    }
}