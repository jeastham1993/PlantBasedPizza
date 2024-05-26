using BackgroundWorkers.IntegrationEvents;
using PlantBasedPizza.OrderManager.Core.Entities;

namespace BackgroundWorkers.Handlers
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
            var order = await this._orderRepository.Retrieve(evt.OrderIdentifier);

            order.AddHistory($"Order collected by driver {evt.DriverName}");
            
            await this._orderRepository.Update(order).ConfigureAwait(false);
        }
    }
}