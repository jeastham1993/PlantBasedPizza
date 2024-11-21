namespace PlantBasedPizza.OrderManager.Core.Entities
{
    public class OrderCompletedEvent
    {
        private readonly string _eventId;

        public OrderCompletedEvent(string orderIdentifier)
        {
            _eventId = Guid.NewGuid().ToString();
            EventDate = DateTime.Now;
            OrderIdentifier = orderIdentifier;
        }

        public string OrderIdentifier { get; private set; }
        
        public string EventName => "order-manager.order-completed";
        public string EventVersion => "v1";

        public string EventId => _eventId;

        public DateTime EventDate { get; }
        public string CorrelationId { get; set; }
    }
}