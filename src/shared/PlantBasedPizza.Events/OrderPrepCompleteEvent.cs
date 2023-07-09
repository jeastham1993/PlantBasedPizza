using System;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Events
{
    public class OrderPrepCompleteEvent : IntegrationEvent, IDomainEvent
    {
        public OrderPrepCompleteEvent(string orderIdentifier)
        {
            this.EventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now;
            this.OrderIdentifier = orderIdentifier;
        }
        
        public override string EventName => "kitchen.prep-complete";
        
        public string EventId { get; }
        
        public DateTime EventDate { get; }
        public string CorrelationId { get; set; }

        public string OrderIdentifier { get; private set; }
    }
}