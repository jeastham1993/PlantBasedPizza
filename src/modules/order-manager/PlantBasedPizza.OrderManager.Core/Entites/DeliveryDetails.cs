using Newtonsoft.Json;

namespace PlantBasedPizza.OrderManager.Core.Entites
{
    public class DeliveryDetails
    {
        [JsonConstructor]
        public DeliveryDetails()
        {
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