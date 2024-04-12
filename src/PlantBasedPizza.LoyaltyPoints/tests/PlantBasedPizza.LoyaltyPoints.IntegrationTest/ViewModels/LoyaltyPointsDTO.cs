using System.Text.Json.Serialization;

namespace PlantBasedPizza.LoyaltyPoints.IntegrationTest.ViewModels;

public class LoyaltyPointsDto
{
    [JsonPropertyName("customerIdentifier")]
    public string CustomerIdentifier { get; set; }
    
    [JsonPropertyName("totalPoints")]
    public decimal TotalPoints { get; set; }
}