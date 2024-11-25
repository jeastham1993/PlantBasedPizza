using System.Diagnostics;
using PlantBasedPizza.Deliver.Core.Entities;

namespace PlantBasedPizza.Deliver.Core.GetDeliveryStatus;

public class GetDeliveryQueryHandler
{
    private readonly IDeliveryRequestRepository _deliveryRequestRepository;

    public GetDeliveryQueryHandler(IDeliveryRequestRepository deliveryRequestRepository)
    {
        _deliveryRequestRepository = deliveryRequestRepository;
    }

    public async Task<DeliveryRequestDto> Handle(GetDeliveryQuery query)
    {
        Activity.Current?.AddTag("orderIdentifier", query.OrderIdentifier);
            
        var deliveryRequest = await _deliveryRequestRepository.GetDeliveryStatusForOrder(query.OrderIdentifier);

        return new DeliveryRequestDto(deliveryRequest);
    }
}