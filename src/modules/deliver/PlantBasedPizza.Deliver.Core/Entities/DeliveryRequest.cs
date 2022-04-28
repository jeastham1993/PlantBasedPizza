using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
        
        [JsonProperty]
        public string OrderIdentifier { get; private set; }
        
        [JsonProperty]
        public string Driver { get; private set; }
        
        public bool AwaitingCollection => !this.DriverCollectedOn.HasValue;
        
        [JsonProperty]
        public Address DeliveryAddress { get; private set; }

        [JsonProperty]
        public DateTime? DriverCollectedOn { get; private set; }

        [JsonProperty]
        public DateTime? DeliveredOn { get; private set; }

        public async Task ClaimDelivery(string driverName, string correlationId = "")
        {
            this.Driver = driverName;
            this.DriverCollectedOn = DateTime.Now;

            await EventManager.Raise(new DriverCollectedOrderEvent(this.OrderIdentifier, driverName)
            {
                CorrelationId = correlationId
            });
        }

        public async Task Deliver(string correlationId = "")
        {
            this.DeliveredOn = DateTime.Now;

            await EventManager.Raise(new OrderDeliveredEvent(this.OrderIdentifier)
            {
                CorrelationId = correlationId
            });
        }
    }
}