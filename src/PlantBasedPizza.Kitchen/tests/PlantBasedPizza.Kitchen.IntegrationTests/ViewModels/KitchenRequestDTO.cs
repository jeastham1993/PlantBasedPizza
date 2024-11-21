using System.Text.Json.Serialization;

namespace PlantBasedPizza.Kitchen.IntegrationTests.ViewModels;

public class KitchenRequestDto
{
    public KitchenRequestDto()
    {
        KitchenRequestId = "";
        OrderIdentifier = "";
    }

    [JsonPropertyName("kitchenRequestId")]
    public string KitchenRequestId { get; set; }
    
    [JsonPropertyName("orderIdentifier")]
    public string OrderIdentifier { get; set; }
    
    [JsonPropertyName("orderReceivedOn")]
    public DateTime OrderReceivedOn { get; set; }
        
    [JsonPropertyName("prepCompleteOn")]
    public DateTime? PrepCompleteOn { get; set; }
        
    [JsonPropertyName("bakeCompleteOn")]
    public DateTime? BakeCompleteOn { get; set; }
        
    [JsonPropertyName("qualityCheckCompleteOn")]
    public DateTime? QualityCheckCompleteOn { get; set; }
}