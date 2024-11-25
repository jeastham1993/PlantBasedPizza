using Microsoft.Extensions.Logging;
using PlantBasedPizza.Deliver.Core.Entities;

namespace PlantBasedPizza.Deliver.Core.OrderReadyForDelivery
{
    public class OrderReadyForDeliveryEventHandler
    {
        private readonly IDeliveryRequestRepository _deliveryRequestRepository;
        private readonly ILogger<OrderReadyForDeliveryEventHandler> _logger;

        public OrderReadyForDeliveryEventHandler(IDeliveryRequestRepository deliveryRequestRepository, ILogger<OrderReadyForDeliveryEventHandler> logger)
        {
            _deliveryRequestRepository = deliveryRequestRepository;
            _logger = logger;
        }
        
        public async Task Handle(OrderReadyForDeliveryEventV1 evt)
        {
            if (evt == null)
            {
                throw new ArgumentNullException(nameof(evt), "Handled event cannot be null");
            }
            
            _logger.LogInformation("Received new ready for delivery event for order {orderIdentifier}", evt.OrderIdentifier);

            var existingDeliveryRequestForOrder =
                await _deliveryRequestRepository.GetDeliveryStatusForOrder(evt.OrderIdentifier);

            if (existingDeliveryRequestForOrder != null)
            {
                _logger.LogInformation("Delivery request for order received, skipping");
                return;
            }
            
            var request = new DeliveryRequest(evt.OrderIdentifier,
                new Address(evt.DeliveryAddressLine1, evt.DeliveryAddressLine2, evt.DeliveryAddressLine3,
                    evt.DeliveryAddressLine4, evt.DeliveryAddressLine5, evt.Postcode));

            await _deliveryRequestRepository.AddNewDeliveryRequest(request);
        }
    }
}