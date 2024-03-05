using PlantBasedPizza.Kitchen.Core.Entities;

namespace PlantBasedPizza.Kitchen.Infrastructure.DataTransfer;

public class KitchenRequestDTO
{
    public KitchenRequestDTO()
    {
        this.KitchenRequestId = "";
        this.OrderIdentifier = "";
    }

    public KitchenRequestDTO(KitchenRequest request)
    {
        this.KitchenRequestId = request.KitchenRequestId;
        this.OrderIdentifier = request.OrderIdentifier;
        this.OrderReceivedOn = request.OrderReceivedOn;
        this.PrepCompleteOn = request.PrepCompleteOn;
        this.BakeCompleteOn = request.BakeCompleteOn;
        this.QualityCheckCompleteOn = request.QualityCheckCompleteOn;
    }

    public string KitchenRequestId { get; set; }
    
    public string OrderIdentifier { get; set; }
    
    public DateTime OrderReceivedOn { get; set; }
        
    public DateTime? PrepCompleteOn { get; set; }
        
    public DateTime? BakeCompleteOn { get; set; }
        
    public DateTime? QualityCheckCompleteOn { get; set; }
}