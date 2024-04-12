using System.Text.Json.Serialization;
using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.OrderManager.Core.CreateDeliveryOrder
{
    public class CreateDeliveryOrder
    {
        [JsonPropertyName("orderIdentifier")]
        public string OrderIdentifier { get; init; } = "";

        [JsonPropertyName("customerIdentifier")]
        public string CustomerIdentifier { get; init; } = "";

        [JsonPropertyName("orderType")]
        public OrderType OrderType => OrderType.Delivery;
        
        [JsonPropertyName("addressLine1")]
        public string AddressLine1 { get; init; } = "";
        
        [JsonPropertyName("addressLine2")]
        public string AddressLine2 { get; init; } = "";

        [JsonPropertyName("addressLine3")]
        public string AddressLine3 { get; init; } = "";

        [JsonPropertyName("addressLine4")]
        public string AddressLine4 { get; init; } = "";

        [JsonPropertyName("addressLine5")]
        public string AddressLine5 { get; init; } = "";

        [JsonPropertyName("postcode")]
        public string Postcode { get; init; } = "";
    }
}