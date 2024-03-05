using System;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Events
{
    public class OrderQualityCheckedEvent : IDomainEvent
    {
        public OrderQualityCheckedEvent(string orderIdentifier)
        {
            this.EventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now;
            this.OrderIdentifier = orderIdentifier;
            this.CorrelationId = CorrelationContext.GetCorrelationId();
        }
        
        public string EventName => "kitchen.quality-checked";
        
        public string EventVersion => "v1";
        
        public string EventId { get; }
        
        public DateTime EventDate { get; }
        public string CorrelationId { get; set; }

        public string OrderIdentifier { get; private set; }
    }
}