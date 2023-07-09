using System;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Events
{
    public class OrderCompletedEvent : IntegrationEvent, IDomainEvent
    {
        private string _eventId;

        public OrderCompletedEvent(string orderIdentifier)
        {
            this._eventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now;
            this.OrderIdentifier = orderIdentifier;
        }

        public string OrderIdentifier { get; private set; }
        
        public override string EventName => "order-manager.order-completed";

        public string EventId => this._eventId;

        public DateTime EventDate { get; }
        public string CorrelationId { get; set; }
    }
}