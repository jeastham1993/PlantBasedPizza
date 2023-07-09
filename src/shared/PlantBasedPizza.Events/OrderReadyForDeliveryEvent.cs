using System;
using System.Text.Json.Serialization;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Events
{
    public class OrderReadyForDeliveryEvent : IntegrationEvent, IDomainEvent
    {
        private string _eventId;
        
        public OrderReadyForDeliveryEvent(string orderIdentifier, string deliveryAddressLine1, string deliveryAddressLine2, string deliveryAddressLine3, string deliveryAddressLine4, string deliveryAddressLine5, string postcode)
        {
            this._eventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now;
            this.OrderIdentifier = orderIdentifier;
            this.DeliveryAddressLine1 = deliveryAddressLine1;
            this.DeliveryAddressLine2 = deliveryAddressLine2;
            this.DeliveryAddressLine3 = deliveryAddressLine3;
            this.DeliveryAddressLine4 = deliveryAddressLine4;
            this.DeliveryAddressLine5 = deliveryAddressLine5;
            this.Postcode = postcode;
        }

        public string OrderIdentifier { get; private set; }
        
        public string DeliveryAddressLine1 { get; private set; }
        
        public string DeliveryAddressLine2 { get; private set; }
        
        public string DeliveryAddressLine3 { get; private set; }
        
        public string DeliveryAddressLine4 { get; private set; }
        
        public string DeliveryAddressLine5 { get; private set; }
        
        public string Postcode { get; private set; }
        
        public override string EventName => "order-manager.ready-for-delivery";

        public string EventId => _eventId;
        
        public DateTime EventDate { get; }
        
        public string CorrelationId { get; set; }
    }
}