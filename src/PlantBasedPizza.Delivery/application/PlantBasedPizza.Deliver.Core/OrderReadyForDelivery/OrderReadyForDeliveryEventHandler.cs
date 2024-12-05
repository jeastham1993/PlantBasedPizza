using Microsoft.Extensions.Logging;
using PlantBasedPizza.Deliver.Core.Entities;
using Saunter.Attributes;

namespace PlantBasedPizza.Deliver.Core.OrderReadyForDelivery
{
    [AsyncApi]
    public class OrderReadyForDeliveryEventHandler
    {
        private readonly IDeliveryRequestRepository _deliveryRequestRepository;
        private readonly ILogger<OrderReadyForDeliveryEventHandler> _logger;

        public OrderReadyForDeliveryEventHandler(IDeliveryRequestRepository deliveryRequestRepository, ILogger<OrderReadyForDeliveryEventHandler> logger)
        {
            _deliveryRequestRepository = deliveryRequestRepository;
            _logger = logger;
        }
        
        [Channel("order.readyForDelivery.v1")]
        [PublishOperation(typeof(OrderReadyForDeliveryEventV1), OperationId = nameof(OrderReadyForDeliveryEventV1))]
        public async Task Handle(OrderReadyForDeliveryEventV1 evt)
        {
            if (evt == null)
            {
                throw new ArgumentNullException(nameof(evt), "Handled event cannot be null");
            }
            
            _logger.LogInformation("Received new ready for delivery event for order {OrderIdentifier}", evt.OrderIdentifier);

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