using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Shared.Events;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Core.Handlers
{
    [AsyncApi]
    public class DriverDeliveredOrderEventHandler(IOrderRepository orderRepository) : Handles<OrderDeliveredEvent>
    {
        [Channel("delivery.order-delivered")] // Creates a Channel
        [SubscribeOperation(typeof(OrderDeliveredEvent), Summary = "Handle an order delivered event.", OperationId = "delivery.order-delivered")]
        public async Task Handle(OrderDeliveredEvent evt)
        {
            var order = await orderRepository.Retrieve(evt.OrderIdentifier);

            order.CompleteOrder();
            
            await orderRepository.Update(order).ConfigureAwait(false);
        }
    }
}