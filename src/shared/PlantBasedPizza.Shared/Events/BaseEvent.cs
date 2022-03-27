using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Shared.Events
{
    public class BaseEvent : IEvent
    {
        public BaseEvent()
        {
            this.CorrelationId = CorrelationContext.GetCorrelationId();
            this.EventDate = DateTime.Now;
        }

        public string CorrelationId { get; set; }

        public DateTime EventDate { get; set; }

        public string Service { get; set; }

        public string EventName { get; }

        public string EventId { get; }
    }
}
