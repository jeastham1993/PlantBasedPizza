using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Deliver.Core.PublicEvents;

namespace PlantBasedPizza.Deliver.Core.AssignDriver;

public class AssignDriverRequestHandler(IDeliveryRequestRepository deliveryRequests)
{
    public async Task<DeliveryRequestDto?> Handle(AssignDriverRequest request)
    {
        var existingDeliveryRequest = await deliveryRequests.GetDeliveryStatusForOrder(request.OrderIdentifier);

        if (existingDeliveryRequest == null) return null;

        await existingDeliveryRequest.ClaimDelivery(request.DriverName);

        await deliveryRequests.UpdateDeliveryRequest(existingDeliveryRequest, [new DriverCollectedOrderEventV1(existingDeliveryRequest)]);

        return new DeliveryRequestDto(existingDeliveryRequest);
    }
}