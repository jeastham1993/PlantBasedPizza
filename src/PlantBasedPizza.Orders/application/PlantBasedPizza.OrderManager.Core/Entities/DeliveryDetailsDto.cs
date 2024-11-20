using System.Text.Json.Serialization;
namespace PlantBasedPizza.OrderManager.Core.Entities;

public record DeliveryDetailsDto
{
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