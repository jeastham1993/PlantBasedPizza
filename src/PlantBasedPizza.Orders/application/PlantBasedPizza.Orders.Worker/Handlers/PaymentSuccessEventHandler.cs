using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;

namespace PlantBasedPizza.Orders.Worker.Handlers
{
    public class PaymentSuccessEventHandler
    {
        private readonly IOrderRepository _orderRepository;

        public PaymentSuccessEventHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        
        public async Task Handle(PaymentSuccessfulEventV1 evt)
        {
            var order = await _orderRepository.Retrieve(evt.OrderIdentifier);

            order.Confirm(evt.Amount);

            await _orderRepository.Update(order);
        }
    }
}