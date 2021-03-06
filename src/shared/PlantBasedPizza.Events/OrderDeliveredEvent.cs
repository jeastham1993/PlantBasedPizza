using System;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Events
{
    public class OrderDeliveredEvent : IDomainEvent
    {
        public OrderDeliveredEvent(string orderIdentifier)
        {
            this.EventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now;
            this.OrderIdentifier = orderIdentifier;
        }
        
        public string EventName => "delivery.order-delivered";
        
        public string EventId { get; }
        
        public DateTime EventDate { get; }
        public string CorrelationId { get; set; }

        public string OrderIdentifier { get; private set; }
    }
}