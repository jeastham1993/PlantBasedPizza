using System.Text.Json.Serialization;

namespace PlantBasedPizza.OrderManager.Core
{
    public class DeliveryDetails
    {
        [JsonConstructor]
        public DeliveryDetails()
        {
            this.AddressLine1 = "";
            this.AddressLine2 = "";
            this.AddressLine3 = "";
            this.AddressLine4 = "";
            this.AddressLine5 = "";
            this.Postcode = "";
        }
        
        public int DeliveryDetailsId { get; set; }
        
        [JsonPropertyName("addressLine1")]
        public string AddressLine1 { get; init; }
        
        [JsonPropertyName("addressLine2")]
        public string AddressLine2 { get; init; }
        
        [JsonPropertyName("addressLine3")]
        public string AddressLine3 { get; init; }
        
        [JsonPropertyName("addressLine4")]
        public string AddressLine4 { get; init; }
        
        [JsonPropertyName("addressLine5")]
        public string AddressLine5 { get; init; }
        
        [JsonPropertyName("postcode")]
        public string Postcode { get; init; }
    }
}