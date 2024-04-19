using System.Text.Json.Serialization;
using PlantBasedPizza.Kitchen.Core.Entities;

namespace PlantBasedPizza.Kitchen.Infrastructure.DataTransfer;

public class KitchenRequestDto
{
    public KitchenRequestDto()
    {
        this.KitchenRequestId = "";
        this.OrderIdentifier = "";
    }

    public KitchenRequestDto(KitchenRequest request)
    {
        this.KitchenRequestId = request.KitchenRequestId;
        this.OrderIdentifier = request.OrderIdentifier;
        this.OrderReceivedOn = request.OrderReceivedOn;
        this.PrepCompleteOn = request.PrepCompleteOn;
        this.BakeCompleteOn = request.BakeCompleteOn;
        this.QualityCheckCompleteOn = request.QualityCheckCompleteOn;
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