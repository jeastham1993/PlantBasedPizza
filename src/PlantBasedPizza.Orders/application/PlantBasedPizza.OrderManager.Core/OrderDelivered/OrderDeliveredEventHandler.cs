using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.OrderManager.Core.OrderDelivered;

public class OrderDeliveredEventHandler(IOrderRepository orderRepository)
{
    public async Task<OrderDto?> Handle(OrderDeliveredEvent evt)
    {
        var order = await orderRepository.Retrieve(evt.OrderIdentifier);

        order.CompleteOrder();
            
        await orderRepository.Update(order).ConfigureAwait(false);

        return new OrderDto(order);
    }
}