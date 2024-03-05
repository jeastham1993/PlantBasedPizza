using System.Text.Json.Serialization;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Deliver.Core.Entities
{
    public class DeliveryRequest
    {
        [JsonConstructor]
        private DeliveryRequest()
        {
        }
        
        public DeliveryRequest(string orderIdentifier, Address deliveryAddress)
        {
            this.OrderIdentifier = orderIdentifier;
            this.DeliveryAddress = deliveryAddress;
        }
        
        [JsonPropertyName("orderIdentifier")]
        public string OrderIdentifier { get; private set; } = "";
        
        [JsonPropertyName("driver")]
        public string Driver { get; private set; } = "";
        
        public bool AwaitingCollection => !this.DriverCollectedOn.HasValue;
        
        [JsonPropertyName("deliveryAddress")]
        public Address DeliveryAddress { get; private set; }

        [JsonPropertyName("driverCollectedOn")]
        public DateTime? DriverCollectedOn { get; private set; }

        [JsonPropertyName("deliveredOn")]
        public DateTime? DeliveredOn { get; private set; }

        public async Task ClaimDelivery(string driverName, string correlationId = "")
        {
            this.Driver = driverName;
            this.DriverCollectedOn = DateTime.Now;

            await DomainEvents.Raise(new DriverCollectedOrderEvent(this.OrderIdentifier, driverName)
            {
                CorrelationId = correlationId
            });
        }

        public async Task Deliver(string correlationId = "")
        {
            this.DeliveredOn = DateTime.Now;

            await DomainEvents.Raise(new OrderDeliveredEvent(this.OrderIdentifier)
            {
                CorrelationId = correlationId
            });
        }
    }
}