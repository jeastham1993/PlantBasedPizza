using PlantBasedPizza.Kitchen.Core.Entities;

namespace PlantBasedPizza.Kitchen.Infrastructure.DataTransfer;

public class KitchenRequestDTO
{
    public KitchenRequestDTO()
    {
    }

    public KitchenRequestDTO(KitchenRequest request)
    {
        this.KitchenRequestId = request.KitchenRequestId;
        this.OrderIdentifier = request.OrderIdentifier;
        this.OrderReceivedOn = request.OrderReceivedOn;
        this.OrderState = request.OrderState.ToString();
        this.PrepCompleteOn = request.PrepCompleteOn;
        this.BakeCompleteOn = request.BakeCompleteOn;
        this.QualithCheckCompleteOn = request.QualithCheckCompleteOn;
    }

    public string KitchenRequestId { get; set; }
    
    public string OrderIdentifier { get; set; }
    
    public DateTime OrderReceivedOn { get; private set; }
    
    public string OrderState { get; private set; }
    
    public DateTime? PrepCompleteOn { get; private set; }
    
    public DateTime? BakeCompleteOn { get; private set; }
    
    public DateTime? QualithCheckCompleteOn { get; private set; }
}