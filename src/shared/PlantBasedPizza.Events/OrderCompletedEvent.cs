using System;
using System.Text.Json.Serialization;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Events
{
    public class OrderCompletedEvent : IDomainEvent
    {
        private readonly string _eventId;

        public OrderCompletedEvent(string customerIdentifier, string orderIdentifier, decimal orderValue)
        {
            this._eventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now;
            this.CustomerIdentifier = customerIdentifier;
            this.OrderIdentifier = orderIdentifier;
            OrderValue = orderValue;
            this.CorrelationId = CorrelationContext.GetCorrelationId();
        }
        
        [JsonPropertyName("customerIdentifier")]
        public string CustomerIdentifier { get; set; }

        [JsonPropertyName("orderIdentifier")]
        public string OrderIdentifier { get; set; }
        
        [JsonPropertyName("orderValue")]
        public decimal OrderValue { get; set; }
        
        public string EventName => "order-manager.order-completed";
        public string EventVersion => "v1";

        public string EventId => this._eventId;

        public DateTime EventDate { get; }
        public string CorrelationId { get; set; }
    }
}