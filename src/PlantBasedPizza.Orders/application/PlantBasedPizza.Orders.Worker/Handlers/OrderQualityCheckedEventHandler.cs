using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.IntegrationEvents;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;

namespace PlantBasedPizza.Orders.Worker.Handlers
{
    public class OrderQualityCheckedEventHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IEventPublisher _eventPublisher;

        public OrderQualityCheckedEventHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        
        public async Task Handle(OrderQualityCheckedEventV1 evt)
        {
            var order = await this._orderRepository.Retrieve(evt.OrderIdentifier);

            order.AddHistory("Order quality checked");

            if (order.OrderType == OrderType.Delivery)
            {
                order.AddHistory("Sending for delivery");

                await this._eventPublisher.Publish(new OrderReadyForDeliveryEventV1()
                {
                    OrderIdentifier = order.OrderIdentifier,
                    DeliveryAddressLine1 = order.DeliveryDetails.AddressLine1,
                    DeliveryAddressLine2 = order.DeliveryDetails.AddressLine2,
                    DeliveryAddressLine3 = order.DeliveryDetails.AddressLine3,
                    DeliveryAddressLine4 = order.DeliveryDetails.AddressLine4,
                    DeliveryAddressLine5 = order.DeliveryDetails.AddressLine5,
                    Postcode = order.DeliveryDetails.Postcode,
                });
            }
            else
            {
                order.IsAwaitingCollection();
            }

            await this._orderRepository.Update(order).ConfigureAwait(false);
        }
    }
}