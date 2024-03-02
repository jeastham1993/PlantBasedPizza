using System.Text.Json.Serialization;
using PlantBasedPizza.OrderManager.Core.Entites;

namespace PlantBasedPizza.OrderManager.Core.Command
{
    public class CreateDeliveryOrder
    {
        [JsonPropertyName("OrderIdentifier")]
        public string OrderIdentifier { get; init; } = "";

        [JsonPropertyName("CustomerIdentifier")]
        public string CustomerIdentifier { get; init; } = "";

        [JsonPropertyName("OrderType")]
        public OrderType OrderType => OrderType.DELIVERY;
        
        [JsonPropertyName("AddressLine1")]
        public string AddressLine1 { get; init; } = "";
        
        [JsonPropertyName("AddressLine2")]
        public string AddressLine2 { get; init; } = "";

        [JsonPropertyName("AddressLine3")]
        public string AddressLine3 { get; init; } = "";

        [JsonPropertyName("AddressLine4")]
        public string AddressLine4 { get; init; } = "";

        [JsonPropertyName("AddressLine5")]
        public string AddressLine5 { get; init; } = "";

        [JsonPropertyName("Postcode")]
        public string Postcode { get; init; } = "";
    }
}