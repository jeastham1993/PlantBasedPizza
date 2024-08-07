using BackgroundWorkers.IntegrationEvents;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Infrastructure.IntegrationEvents;

namespace BackgroundWorkers.Handlers
{
    public class OrderQualityCheckedEventHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderEventPublisher _eventPublisher;

        public OrderQualityCheckedEventHandler(IOrderRepository orderRepository, IOrderEventPublisher eventPublisher)
        {
            _orderRepository = orderRepository;
            _eventPublisher = eventPublisher;
        }
        
        public async Task Handle(OrderQualityCheckedEventV1 evt)
        {
            var order = await this._orderRepository.RetrieveByOrderId(evt.OrderIdentifier);

            order.AddHistory("Order quality checked");

            if (order.OrderType == OrderType.Delivery)
            {
                order.AddHistory("Sending for delivery");

                await this._eventPublisher.PublishOrderReadyForDeliveryEventV1(order);
            }
            else
            {
                order.IsAwaitingCollection();
            }

            await this._orderRepository.Update(order).ConfigureAwait(false);
        }
    }
}