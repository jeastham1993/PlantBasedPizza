using PlantBasedPizza.Deliver.Core.Entities;

namespace PlantBasedPizza.Deliver.Core.AssignDriver;

public class AssignDriverRequestHandler(IDeliveryRequestRepository deliveryRequests, IDeliveryEventPublisher eventPublisher)
{
    public async Task<DeliveryRequestDto?> Handle(AssignDriverRequest request)
    {
        var existingDeliveryRequest = await deliveryRequests.GetDeliveryStatusForOrder(request.OrderIdentifier);

        if (existingDeliveryRequest == null) return null;

        await existingDeliveryRequest.ClaimDelivery(request.DriverName);

        await deliveryRequests.UpdateDeliveryRequest(existingDeliveryRequest);
        await eventPublisher.PublishDriverOrderCollectedEventV1(new DriverCollectedOrderEventV1(existingDeliveryRequest));

        return new DeliveryRequestDto(existingDeliveryRequest);
    }
}