using System.Text.Json.Serialization;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace PlantBasedPizza.Events
{
    public class OrderCompletedEvent : IntegrationEvent, IDomainEvent
    {
        private readonly string _eventId;

        public OrderCompletedEvent(string customerIdentifier, string orderIdentifier, decimal orderValue)
        {
            this._eventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now.ToUniversalTime();
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
        
        public override string EventName => "orders.order-completed";
        public override string EventVersion => "v1";
        public override Uri Source => new Uri("https://monolith.plantbasedpizza");
        public override string AsString()
        {
            return JsonSerializer.Serialize(this);
        }

        public string EventId => this._eventId;

        public DateTime EventDate { get; }
        public string CorrelationId { get; set; }
    }
}