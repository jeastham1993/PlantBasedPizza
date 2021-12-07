using System.Threading.Tasks;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entites;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.OrderManager.Core.Handlers
{
    public class OrderQualityCheckedEventHandler : Handles<OrderQualityCheckedEvent>
    {
        private readonly IOrderRepository _orderRepository;

        public OrderQualityCheckedEventHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task Handle(OrderQualityCheckedEvent evt)
        {
            var order = await this._orderRepository.Retrieve(evt.OrderIdentifier);

            order.AddHistory("Order quality checked");

            if (order.OrderType == OrderType.DELIVERY)
            {
                order.AddHistory("Sending for delivery");

                await DomainEvents.Raise(new OrderReadyForDeliveryEvent(order.OrderIdentifier,
                    order.DeliveryDetails.AddressLine1, order.DeliveryDetails.AddressLine2,
                    order.DeliveryDetails.AddressLine3, order.DeliveryDetails.AddressLine4,
                    order.DeliveryDetails.AddressLine5, order.DeliveryDetails.Postcode));
            }
            else
            {
                order.IsAwaitingCollection();
            }

            await this._orderRepository.Update(order).ConfigureAwait(false);
        }
    }
}