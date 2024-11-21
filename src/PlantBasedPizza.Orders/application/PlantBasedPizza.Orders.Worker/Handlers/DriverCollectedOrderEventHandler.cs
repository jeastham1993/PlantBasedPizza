using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;

namespace PlantBasedPizza.Orders.Worker.Handlers
{
    public class DriverCollectedOrderEventHandler
    {
        private readonly IOrderRepository _orderRepository;

        public DriverCollectedOrderEventHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        
        public async Task Handle(DriverCollectedOrderEventV1 evt)
        {
            var order = await _orderRepository.Retrieve(evt.OrderIdentifier);

            order.AddHistory($"Order collected by driver {evt.DriverName}");
            
            await _orderRepository.Update(order).ConfigureAwait(false);
        }
    }
}