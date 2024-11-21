using System.Text.Json.Serialization;
using PlantBasedPizza.Kitchen.Core.Entities;

namespace PlantBasedPizza.Kitchen.Infrastructure.DataTransfer;

public class KitchenRequestDto
{
    public KitchenRequestDto()
    {
        KitchenRequestId = "";
        OrderIdentifier = "";
    }

    public KitchenRequestDto(KitchenRequest request)
    {
        KitchenRequestId = request.KitchenRequestId;
        OrderIdentifier = request.OrderIdentifier;
        OrderReceivedOn = request.OrderReceivedOn;
        PrepCompleteOn = request.PrepCompleteOn;
        BakeCompleteOn = request.BakeCompleteOn;
        QualityCheckCompleteOn = request.QualityCheckCompleteOn;
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