using System;
using PlantBasedPizza.Shared.Events;
using Saunter.Attributes;

namespace PlantBasedPizza.Events
{
    public class DriverCollectedOrderEvent : IDomainEvent
    {
        public DriverCollectedOrderEvent(string orderIdentifier, string driverName)
        {
            this.EventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now;
            this.OrderIdentifier = orderIdentifier;
            this.DriverName = driverName;
        }
        
        public string EventName => "delivery.driver-collected";
        
        public string EventId { get; }
        
        public DateTime EventDate { get; }
        public string CorrelationId { get; set; }

        public string OrderIdentifier { get; private set; }
        
        public string DriverName { get; private set; }
    }
}