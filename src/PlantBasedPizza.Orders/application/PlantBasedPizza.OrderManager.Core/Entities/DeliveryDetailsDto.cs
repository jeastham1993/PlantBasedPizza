using Newtonsoft.Json;

namespace PlantBasedPizza.OrderManager.Core.Entities;

public record DeliveryDetailsDto
{
    [JsonProperty("addressLine1")]
    public string AddressLine1 { get; init; } = "";
        
    [JsonProperty("addressLine2")]
    public string AddressLine2 { get; init; } = "";
        
    [JsonProperty("addressLine3")]
    public string AddressLine3 { get; init; } = "";
        
    [JsonProperty("addressLine4")]
    public string AddressLine4 { get; init; } = "";
        
    [JsonProperty("addressLine5")]
    public string AddressLine5 { get; init; } = "";
        
    [JsonProperty("postcode")]
    public string Postcode { get; init; } = "";
}