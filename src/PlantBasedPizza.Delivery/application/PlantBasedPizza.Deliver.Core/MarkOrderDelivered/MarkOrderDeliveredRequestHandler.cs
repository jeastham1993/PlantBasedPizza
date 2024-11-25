using PlantBasedPizza.Deliver.Core.AssignDriver;
using PlantBasedPizza.Deliver.Core.Entities;

namespace PlantBasedPizza.Deliver.Core.MarkOrderDelivered;

public class MarkOrderDeliveredRequestHandler(IDeliveryRequestRepository deliveryRequests, IDeliveryEventPublisher eventPublisher)
{
    public async Task<DeliveryRequestDto?> Handle(MarkOrderDeliveredRequest request)
    {
        var existingDeliveryRequest = await deliveryRequests.GetDeliveryStatusForOrder(request.OrderIdentifier);

        if (existingDeliveryRequest == null) return null;

        await existingDeliveryRequest.Deliver();
            
        await deliveryRequests.UpdateDeliveryRequest(existingDeliveryRequest);
        await eventPublisher.PublishDriverDeliveredOrderEventV1(new DriverDeliveredOrderEventV1(existingDeliveryRequest));

        return new DeliveryRequestDto(existingDeliveryRequest);
    }
}