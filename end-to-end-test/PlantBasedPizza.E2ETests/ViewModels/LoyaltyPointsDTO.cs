using System.Text.Json.Serialization;

namespace PlantBasedPizza.E2ETests.ViewModels;

public class LoyaltyPointsDto
{
    [JsonPropertyName("customerIdentifier")]
    public string CustomerIdentifier { get; set; }
    
    [JsonPropertyName("totalPoints")]
    public decimal TotalPoints { get; set; }
}