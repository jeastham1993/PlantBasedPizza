using System.Text.Json.Serialization;

namespace PlantBasedPizza.LoyaltyPoints.Shared.Core;

public class LoyaltyPointsDto
{
    public LoyaltyPointsDto(decimal totalPoints)
    {
        TotalPoints = totalPoints;
        History = new List<LoyaltyPointHistoryDto>();
    }
    
    public LoyaltyPointsDto(CustomerLoyaltyPoints points)
    {
        TotalPoints = points.TotalPoints;
        History = points.History.Select(history => new LoyaltyPointHistoryDto()
        {
            DateTime = history.DateTime,
            PointsAdded = history.PointsAdded,
            OrderIdentifier = history.OrderIdentifier,
            OrderValue = history.OrderValue
        }).ToList();
    }
    
    [JsonPropertyName("totalPoints")]
    public decimal TotalPoints { get; set; }
    
    [JsonPropertyName("history")]
    public List<LoyaltyPointHistoryDto> History { get; set; }
}