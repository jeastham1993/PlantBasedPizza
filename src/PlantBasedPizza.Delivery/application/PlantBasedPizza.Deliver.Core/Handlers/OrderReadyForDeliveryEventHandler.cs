using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Deliver.Core.IntegrationEvents;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Deliver.Core.Handlers
{
    public class OrderReadyForDeliveryEventHandler
    {
        private readonly IDeliveryRequestRepository _deliveryRequestRepository;

        public OrderReadyForDeliveryEventHandler(IDeliveryRequestRepository deliveryRequestRepository)
        {
            _deliveryRequestRepository = deliveryRequestRepository;
        }
        
        public async Task Handle(OrderReadyForDeliveryEventV1 evt)
        {
            if (evt == null)
            {
                throw new ArgumentNullException(nameof(evt), "Handled event cannot be null");
            }

            var existingDeliveryRequestForOrder =
                await this._deliveryRequestRepository.GetDeliveryStatusForOrder(evt.OrderIdentifier);

            if (existingDeliveryRequestForOrder != null)
            {
                return;
            }

            var request = new DeliveryRequest(evt.OrderIdentifier,
                new Address(evt.DeliveryAddressLine1, evt.DeliveryAddressLine2, evt.DeliveryAddressLine3,
                    evt.DeliveryAddressLine4, evt.DeliveryAddressLine5, evt.Postcode));

            await this._deliveryRequestRepository.AddNewDeliveryRequest(request);
        }
    }
}