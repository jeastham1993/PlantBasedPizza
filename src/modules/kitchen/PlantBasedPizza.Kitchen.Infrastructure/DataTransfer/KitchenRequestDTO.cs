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
    }

    public string KitchenRequestId { get; set; }
}