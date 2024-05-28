using BackgroundWorkers.IntegrationEvents;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Infrastructure.IntegrationEvents;

namespace BackgroundWorkers.Handlers
{
    public class PaymentSuccessfulEventHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderEventPublisher _eventPublisher;

        public PaymentSuccessfulEventHandler(IOrderRepository orderRepository, IOrderEventPublisher eventPublisher)
        {
            _orderRepository = orderRepository;
            _eventPublisher = eventPublisher;
        }
        
        public async Task Handle(PaymentSuccessfulEventV1 evt)
        {
            var order = await this._orderRepository.Retrieve(evt.OrderIdentifier);
            
            order.AddHistory("Payment successful");
            
            await this._orderRepository.Update(order);

            await this._eventPublisher.PublishOrderConfirmedEventV1(order);
        }
    }
}