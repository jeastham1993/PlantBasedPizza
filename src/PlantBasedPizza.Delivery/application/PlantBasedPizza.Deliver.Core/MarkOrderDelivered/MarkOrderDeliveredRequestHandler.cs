using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Deliver.Core.PublicEvents;

namespace PlantBasedPizza.Deliver.Core.MarkOrderDelivered;

public class MarkOrderDeliveredRequestHandler(IDeliveryRequestRepository deliveryRequests)
{
    public async Task<DeliveryRequestDto?> Handle(MarkOrderDeliveredRequest request)
    {
        var existingDeliveryRequest = await deliveryRequests.GetDeliveryStatusForOrder(request.OrderIdentifier);

        if (existingDeliveryRequest == null) return null;

        await existingDeliveryRequest.Deliver();
            
        await deliveryRequests.UpdateDeliveryRequest(existingDeliveryRequest, [new DriverDeliveredOrderEventV1(existingDeliveryRequest)]);

        return new DeliveryRequestDto(existingDeliveryRequest);
    }
}