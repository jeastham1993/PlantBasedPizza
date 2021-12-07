using System;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Events
{
    public class OrderPrepCompleteEvent : IDomainEvent
    {
        public OrderPrepCompleteEvent(string orderIdentifier)
        {
            this.EventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now;
            this.OrderIdentifier = orderIdentifier;
        }
        
        public string EventName => "kitchen.prep-complete";
        
        public string EventId { get; }
        
        public DateTime EventDate { get; }
        
        public string OrderIdentifier { get; private set; }
    }
}