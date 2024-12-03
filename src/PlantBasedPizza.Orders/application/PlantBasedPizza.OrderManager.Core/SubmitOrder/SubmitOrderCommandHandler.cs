using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Core.SubmitOrder;

public class SubmitOrderCommandHandler(IOrderRepository orderRepository)
{
    public async Task<OrderDto> Handle(SubmitOrderCommand command)
    {
        var order = await orderRepository.Retrieve(command.OrderIdentifier);

        if (order.CustomerIdentifier != command.CustomerIdentifier)
            throw new OrderNotFoundException(command.OrderIdentifier);

        order.SubmitOrder();

        await orderRepository.Update(order);


        return new OrderDto(order);
    }
}