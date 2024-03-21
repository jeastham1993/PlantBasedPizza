using System.Text.Json.Serialization;

namespace PlantBasedPizza.LoyaltyPoints.Core;

public class LoyaltyPointsDTO
{
    public LoyaltyPointsDTO(CustomerLoyaltyPoints points)
    {
        this.CustomerIdentifier = points.CustomerId;
        this.TotalPoints = points.TotalPoints;
    }
    
    [JsonPropertyName("customerIdentifier")]
    public string CustomerIdentifier { get; set; }
    
    [JsonPropertyName("totalPoints")]
    public decimal TotalPoints { get; set; }
}