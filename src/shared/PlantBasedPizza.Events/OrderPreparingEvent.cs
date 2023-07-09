using System;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Events
{
    public class OrderPreparingEvent : IntegrationEvent, IDomainEvent
    {
        public OrderPreparingEvent(string orderIdentifier)
        {
            this.EventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now;
            this.OrderIdentifier = orderIdentifier;
        }
        
        public override string EventName => "kitchen.prep-started";
        
        public string EventId { get; }
        
        public DateTime EventDate { get; }
        public string CorrelationId { get; set; }

        public string OrderIdentifier { get; private set; }
    }
}