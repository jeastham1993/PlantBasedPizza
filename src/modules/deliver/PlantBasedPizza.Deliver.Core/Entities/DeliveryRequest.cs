using System;
using System.Threading.Tasks;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Deliver.Core.Entities
{
    public class DeliveryRequest
    {
        public DeliveryRequest(string orderIdentifier, Address deliveryAddress)
        {
            this.OrderIdentifier = orderIdentifier;
            this.DeliveryAddress = deliveryAddress;
        }
        
        public string OrderIdentifier { get; private set; }
        
        public string Driver { get; private set; }

        public bool AwaitingCollection => !this.DriverCollectedOn.HasValue;
        
        public Address DeliveryAddress { get; private set; }

        public DateTime? DriverCollectedOn { get; private set; }

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