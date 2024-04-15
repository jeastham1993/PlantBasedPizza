using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;

namespace PlantBasedPizza.Orders.Worker.Handlers
{
    public class OrderBakedEventHandler
    {
        private readonly IOrderRepository _orderRepository;

        public OrderBakedEventHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        
        public async Task Handle(OrderBakedEventV1 evt)
        {
            var order = await this._orderRepository.Retrieve(evt.OrderIdentifier);

            order.AddHistory("Order baked");

            await this._orderRepository.Update(order);
        }
    }
}