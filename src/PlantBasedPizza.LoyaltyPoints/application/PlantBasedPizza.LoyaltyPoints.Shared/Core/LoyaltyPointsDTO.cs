using System.Text.Json.Serialization;

namespace PlantBasedPizza.LoyaltyPoints.Shared.Core;

public class LoyaltyPointsDto
{
    public LoyaltyPointsDto(CustomerLoyaltyPoints points)
    {
        this.CustomerIdentifier = points.CustomerId;
        this.TotalPoints = points.TotalPoints;
    }
    
    [JsonPropertyName("customerIdentifier")]
    public string CustomerIdentifier { get; set; }
    
    [JsonPropertyName("totalPoints")]
    public decimal TotalPoints { get; set; }
}