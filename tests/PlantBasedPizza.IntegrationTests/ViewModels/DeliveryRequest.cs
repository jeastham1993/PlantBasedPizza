using System;

namespace PlantBasedPizza.IntegrationTests.ViewModels
{
    public class DeliveryRequest
    {
        public string OrderIdentifier { get; set; }
        
        public string Driver { get; set; }

        public bool AwaitingCollection { get; set; }

        public Address DeliveryAddress { get; set; }

        public DateTime? DriverCollectedOn { get; set; }

        public DateTime? DeliveredOn { get; set; }
    }
}