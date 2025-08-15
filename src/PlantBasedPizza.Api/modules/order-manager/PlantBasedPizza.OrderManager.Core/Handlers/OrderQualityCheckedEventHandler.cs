using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Events;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Core.Handlers
{
    [AsyncApi]
    public class OrderQualityCheckedEventHandler : Handles<OrderQualityCheckedEvent>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IDomainEventDispatcher _eventDispatcher;

        public OrderQualityCheckedEventHandler(IOrderRepository orderRepository, IDomainEventDispatcher eventDispatcher)
        {
            _orderRepository = orderRepository;
            _eventDispatcher = eventDispatcher;
        }

        [Channel("kitchen.quality-checked")] // Creates a Channel
        [SubscribeOperation(typeof(OrderQualityCheckedEvent), Summary = "Handle an order quality event.", OperationId = "kitchen.quality-checked")]
        public async Task Handle(OrderQualityCheckedEvent evt)
        {
            var order = await this._orderRepository.Retrieve(evt.OrderIdentifier);

            order.AddHistory("Order quality checked");

            if (order.OrderType == OrderType.Delivery)
            {
                order.AddHistory("Sending for delivery");

                await _eventDispatcher.PublishAsync(new OrderReadyForDeliveryEvent(order.OrderIdentifier,
                    order.DeliveryDetails.AddressLine1, order.DeliveryDetails.AddressLine2,
                    order.DeliveryDetails.AddressLine3, order.DeliveryDetails.AddressLine4,
                    order.DeliveryDetails.AddressLine5, order.DeliveryDetails.Postcode));
            }
            else
            {
                order.MarkAsAwaitingCollection();
            }

            await this._orderRepository.Update(order).ConfigureAwait(false);
        }
    }
}