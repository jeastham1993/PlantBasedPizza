using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Events;
using Saunter.Attributes;

namespace PlantBasedPizza.Deliver.Core.Handlers
{
    [AsyncApi]
    public class OrderReadyForDeliveryEventHandler(
        IDeliveryRequestRepository deliveryRequestRepository,
        ILogger<OrderReadyForDeliveryEventHandler> logger)
        : Handles<OrderReadyForDeliveryEvent>
    {
        [Channel("order-manager.ready-for-delivery")] // Creates a Channel
        [SubscribeOperation(typeof(OrderReadyForDeliveryEvent), Summary = "Handle an order ready for delivery event.", OperationId = "order-manager.ready-for-delivery")]
        public async Task Handle(OrderReadyForDeliveryEvent evt)
        {
            if (evt == null)
            {
                throw new ArgumentNullException(nameof(evt), "Handled event cannot be null");
            }
            
            logger.LogInformation($"Received new ready for delivery event for order {evt.OrderIdentifier}");

            var existingDeliveryRequestForOrder =
                await deliveryRequestRepository.GetDeliveryStatusForOrder(evt.OrderIdentifier);

            if (existingDeliveryRequestForOrder != null)
            {
                logger.LogInformation("Delivery request for order received, skipping");
                return;
            }

            logger.LogInformation("Creating and storing delivery request");

            var request = new DeliveryRequest(evt.OrderIdentifier,
                new Address(evt.DeliveryAddressLine1, evt.DeliveryAddressLine2, evt.DeliveryAddressLine3,
                    evt.DeliveryAddressLine4, evt.DeliveryAddressLine5, evt.Postcode));

            await deliveryRequestRepository.AddNewDeliveryRequest(request);

            logger.LogInformation("Delivery request added");
        }
    }
}