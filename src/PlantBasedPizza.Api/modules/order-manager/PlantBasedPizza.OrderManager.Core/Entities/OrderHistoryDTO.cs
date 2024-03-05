using System.Text.Json.Serialization;

namespace PlantBasedPizza.OrderManager.Core.Entities;

public record OrderHistoryDto
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";
        
    [JsonPropertyName("historyDate")]
    public DateTime HistoryDate { get; set; }
}