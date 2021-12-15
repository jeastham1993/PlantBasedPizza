using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Deliver.Core.Handlers
{
    public class OrderReadyForDeliveryEventHandler : Handles<OrderReadyForDeliveryEvent>
    {
        private readonly IDeliveryRequestRepository _deliveryRequestRepository;
        private readonly IObservabilityService _logger;

        public OrderReadyForDeliveryEventHandler(IDeliveryRequestRepository deliveryRequestRepository, IObservabilityService logger)
        {
            _deliveryRequestRepository = deliveryRequestRepository;
            _logger = logger;
        }

        public async Task Handle(OrderReadyForDeliveryEvent evt)
        {
            if (evt == null)
            {
                throw new ArgumentNullException(nameof(evt), "Handled event cannot be null");
            }

            var existingDeliveryRequestForOrder =
                await this._deliveryRequestRepository.GetDeliveryStatusForOrder(evt.OrderIdentifier);

            if (existingDeliveryRequestForOrder != null)
            {
                this._logger.Info(evt.CorrelationId, "Delivery request for order received, skipping");
                return;
            }

            var request = new DeliveryRequest(evt.OrderIdentifier,
                new Address(evt.DeliveryAddressLine1, evt.DeliveryAddressLine2, evt.DeliveryAddressLine3,
                    evt.DeliveryAddressLine4, evt.DeliveryAddressLine5, evt.Postcode));

            await this._deliveryRequestRepository.AddNewDeliveryRequest(request);
        }
    }
}