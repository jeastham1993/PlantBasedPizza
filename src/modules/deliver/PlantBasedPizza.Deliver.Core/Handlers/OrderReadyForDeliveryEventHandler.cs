using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;
using Saunter.Attributes;

namespace PlantBasedPizza.Deliver.Core.Handlers
{
    [AsyncApi]
    public class OrderReadyForDeliveryEventHandler : Handles<OrderReadyForDeliveryEvent>
    {
        private readonly IDeliveryRequestRepository _deliveryRequestRepository;
        private readonly IObservabilityService _logger;

        public OrderReadyForDeliveryEventHandler(IDeliveryRequestRepository deliveryRequestRepository, IObservabilityService logger)
        {
            _deliveryRequestRepository = deliveryRequestRepository;
            _logger = logger;
        }

        [Channel("order-manager.ready-for-delivery")] // Creates a Channel
        [SubscribeOperation(typeof(OrderReadyForDeliveryEvent), Summary = "Handle an order ready for delivery event.", OperationId = "order-manager.ready-for-delivery")]
        public async Task Handle(OrderReadyForDeliveryEvent evt)
        {
            if (evt == null)
            {
                throw new ArgumentNullException(nameof(evt), "Handled event cannot be null");
            }
            
            this._logger.Info($"Received new ready for delivery event for order {evt.OrderIdentifier}");

            var existingDeliveryRequestForOrder =
                await this._deliveryRequestRepository.GetDeliveryStatusForOrder(evt.OrderIdentifier);

            if (existingDeliveryRequestForOrder != null)
            {
                this._logger.Info("Delivery request for order received, skipping");
                return;
            }

            this._logger.Info("Creating and storing delivery request");

            var request = new DeliveryRequest(evt.OrderIdentifier,
                new Address(evt.DeliveryAddressLine1, evt.DeliveryAddressLine2, evt.DeliveryAddressLine3,
                    evt.DeliveryAddressLine4, evt.DeliveryAddressLine5, evt.Postcode));

            await this._deliveryRequestRepository.AddNewDeliveryRequest(request);

            this._logger.Info("Delivery request added");
        }
    }
}