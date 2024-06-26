using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.OrderManager.Core.Entities
{
    public class OrderSubmittedEvent : IDomainEvent
    {
        private readonly string _eventId;

        public OrderSubmittedEvent(string orderIdentifier)
        {
            this._eventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now;
            this.OrderIdentifier = orderIdentifier;
            this.CorrelationId = CorrelationContext.GetCorrelationId();
        }

        public string OrderIdentifier { get; private set; }
        
        public string EventName => "order-manager.order-submitted";
        
        public string EventVersion => "v1";

        public string EventId => this._eventId;

        public DateTime EventDate { get; }
        public string CorrelationId { get; set; }
    }
}