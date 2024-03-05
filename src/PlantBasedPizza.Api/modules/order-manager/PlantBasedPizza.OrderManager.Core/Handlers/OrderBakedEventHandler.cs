using System.Threading.Tasks;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.Shared.Events;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Core.Handlers
{
    [AsyncApi]
    public class OrderBakedEventHandler : Handles<OrderBakedEvent>
    {
        private readonly IOrderRepository _orderRepository;

        public OrderBakedEventHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [Channel("kitchen.baked")] // Creates a Channel
        [SubscribeOperation(typeof(OrderBakedEvent), Summary = "Handle an order baked event.", OperationId = "kitchen.baked")]
        public async Task Handle(OrderBakedEvent evt)
        {
            var order = await this._orderRepository.Retrieve(evt.OrderIdentifier);

            order.AddHistory("Order baked");

            await this._orderRepository.Update(order);
        }
    }
}