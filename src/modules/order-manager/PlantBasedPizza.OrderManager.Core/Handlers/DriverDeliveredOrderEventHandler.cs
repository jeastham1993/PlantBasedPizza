using System.Threading.Tasks;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Core.Handlers
{
    [AsyncApi]
    public class DriverDeliveredOrderEventHandler : Handles<OrderDeliveredEvent>
    {
        private readonly IOrderRepository _orderRepository;

        public DriverDeliveredOrderEventHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [Channel("delivery.order-delivered")] // Creates a Channel
        [SubscribeOperation(typeof(OrderDeliveredEvent), Summary = "Handle an order delivered event.", OperationId = "delivery.order-delivered")]
        public async Task Handle(OrderDeliveredEvent evt)
        {
            var order = await this._orderRepository.Retrieve(evt.OrderIdentifier);

            order.CompleteOrder();
            
            await this._orderRepository.Update(order).ConfigureAwait(false);
        }
    }
}