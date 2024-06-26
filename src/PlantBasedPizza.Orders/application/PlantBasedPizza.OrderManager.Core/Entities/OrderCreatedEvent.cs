using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.OrderManager.Core.Entities
{
    public class OrderCreatedEvent : IDomainEvent
    {
        private readonly string _eventId;

        public OrderCreatedEvent(string orderIdentifier)
        {
            this._eventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now;
            this.OrderIdentifier = orderIdentifier;
            this.CorrelationId = CorrelationContext.GetCorrelationId();
        }

        public string OrderIdentifier { get; private set; }
        
        public string EventName => "order-manager.order-created";
        
        public string EventVersion => "v1";

        public string EventId => this._eventId;

        public DateTime EventDate { get; }
        public string CorrelationId { get; set; }
    }
}