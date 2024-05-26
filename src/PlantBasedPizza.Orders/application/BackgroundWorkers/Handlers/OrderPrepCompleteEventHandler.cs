using BackgroundWorkers.IntegrationEvents;
using PlantBasedPizza.OrderManager.Core.Entities;

namespace BackgroundWorkers.Handlers
{
    public class OrderPrepCompleteEventHandler
    {
        private readonly IOrderRepository _orderRepository;

        public OrderPrepCompleteEventHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        
        public async Task Handle(OrderPrepCompleteEventV1 evt)
        {
            var order = await this._orderRepository.Retrieve(evt.OrderIdentifier);
            
            order.AddHistory("Order prep completed");
            
            await this._orderRepository.Update(order);
        }
    }
}