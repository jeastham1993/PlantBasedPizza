using System;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Events
{
    public class OrderSubmittedEvent : IDomainEvent
    {
        private string _eventId;

        public OrderSubmittedEvent(string orderIdentifier)
        {
            this._eventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now;
            this.OrderIdentifier = orderIdentifier;
        }

        public string OrderIdentifier { get; private set; }
        
        public string EventName => "order-manager.order-submitted";

        public string EventId => this._eventId;

        public DateTime EventDate { get; }
        public string CorrelationId { get; set; }
    }
}