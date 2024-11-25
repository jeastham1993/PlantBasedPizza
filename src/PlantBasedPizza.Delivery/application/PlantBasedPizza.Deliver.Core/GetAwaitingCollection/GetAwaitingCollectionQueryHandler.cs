using PlantBasedPizza.Deliver.Core.Entities;

namespace PlantBasedPizza.Deliver.Core.GetAwaitingCollection;

public class GetAwaitingCollectionQueryHandler
{
    private readonly IDeliveryRequestRepository _deliveryRequestRepository;

    public GetAwaitingCollectionQueryHandler(IDeliveryRequestRepository deliveryRequestRepository)
    {
        _deliveryRequestRepository = deliveryRequestRepository;
    }

    public async Task<IEnumerable<DeliveryRequestDto>> Handle(GetAwaitingCollectionQuery query)
    {
        var deliveryRequest = await _deliveryRequestRepository.GetAwaitingDriver();

        return deliveryRequest.Select(request => new DeliveryRequestDto(request));
    }
}