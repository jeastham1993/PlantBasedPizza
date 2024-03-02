using System;
using System.Text.Json.Serialization;

namespace PlantBasedPizza.IntegrationTests.ViewModels
{
    public class DeliveryRequest
    {
        [JsonPropertyName("orderIdentifier")]
        public string OrderIdentifier { get; set; }
        
        [JsonPropertyName("driver")]
        public string Driver { get; set; }

        [JsonPropertyName("awaitingCollection")]
        public bool AwaitingCollection { get; set; }

        [JsonPropertyName("deliveryAddress")]
        public Address DeliveryAddress { get; set; }

        [JsonPropertyName("driverCollectedOn")]
        public DateTime? DriverCollectedOn { get; set; }

        [JsonPropertyName("deliveredOn")]
        public DateTime? DeliveredOn { get; set; }
    }
}