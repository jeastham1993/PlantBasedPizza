using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.OrderManager.Infrastructure.IntegrationEvents;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;

namespace PlantBasedPizza.Orders.Worker.Handlers
{
    public class DriverDeliveredOrderEventHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IEventPublisher _eventPublisher;

        public DriverDeliveredOrderEventHandler(IOrderRepository orderRepository, ILoyaltyPointService loyaltyPointService, IEventPublisher eventPublisher)
        {
            _orderRepository = orderRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task Handle(DriverDeliveredOrderEventV1 evt)
        {
            var order = await this._orderRepository.Retrieve(evt.OrderIdentifier);

            order.CompleteOrder();
            
            await this._orderRepository.Update(order).ConfigureAwait(false);

            await this._eventPublisher.Publish(new OrderCompletedIntegrationEventV1()
            {
                OrderIdentifier = order.OrderIdentifier,
                CustomerIdentifier = order.CustomerIdentifier,
                OrderValue = order.TotalPrice
            });
        }
    }
}