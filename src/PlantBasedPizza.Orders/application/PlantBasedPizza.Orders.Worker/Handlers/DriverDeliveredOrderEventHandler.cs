using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;

namespace PlantBasedPizza.Orders.Worker.Handlers
{
    public class DriverDeliveredOrderEventHandler
    {
        private readonly IOrderRepository _orderRepository;

        public DriverDeliveredOrderEventHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task Handle(DriverDeliveredOrderEventV1 evt)
        {
            var order = await _orderRepository.Retrieve(evt.OrderIdentifier);

            order.CompleteOrder();
            
            await _orderRepository.Update(order).ConfigureAwait(false);
        }
    }
}