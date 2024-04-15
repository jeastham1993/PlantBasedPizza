using Newtonsoft.Json;

namespace PlantBasedPizza.OrderManager.Core.Entities
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
        
        [JsonProperty]
        public string AddressLine1 { get; init; }
        
        [JsonProperty]
        public string AddressLine2 { get; init; }
        
        [JsonProperty]
        public string AddressLine3 { get; init; }
        
        [JsonProperty]
        public string AddressLine4 { get; init; }
        
        [JsonProperty]
        public string AddressLine5 { get; init; }
        
        [JsonProperty]
        public string Postcode { get; init; }
    }
}