using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Events
{
    public class OrderPrepCompleteEvent : IDomainEvent
    {
        public OrderPrepCompleteEvent(string orderIdentifier)
        {
            this.EventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now.ToUniversalTime();
            this.OrderIdentifier = orderIdentifier;
            this.CorrelationId = CorrelationContext.GetCorrelationId();
        }
        
        public string EventName => "kitchen.prep-complete";
        
        public string EventVersion => "v1";
        
        public string EventId { get; }
        
        public DateTime EventDate { get; }
        public string CorrelationId { get; set; }

        public string OrderIdentifier { get; private set; }
    }
}