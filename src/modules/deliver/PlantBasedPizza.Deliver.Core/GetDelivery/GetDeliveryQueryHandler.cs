using System.Diagnostics;
using PlantBasedPizza.Deliver.Core.Entities;

namespace PlantBasedPizza.Deliver.Core.GetDelivery;

public class GetDeliveryQueryHandler
{
    private readonly IDeliveryRequestRepository _deliveryRequestRepository;

    public GetDeliveryQueryHandler(IDeliveryRequestRepository deliveryRequestRepository)
    {
        _deliveryRequestRepository = deliveryRequestRepository;
    }

    public async Task<DeliveryRequestDTO> Handle(GetDeliveryQuery query)
    {
        Activity.Current?.AddTag("orderIdentifier", query.OrderIdentifier);
            
        var deliveryRequest = await this._deliveryRequestRepository.GetDeliveryStatusForOrder(query.OrderIdentifier);

        return new DeliveryRequestDTO(deliveryRequest);
    }
}