using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.OrderManager.Core.OrderSubmitted;

public class SubmitOrderCommandHandler(IOrderRepository orderRepository)
{
    public async Task<OrderDto> Handle(SubmitOrderCommand command)
    {
        var order = await orderRepository.Retrieve(command.OrderIdentifier);
            
        if (order.CustomerIdentifier != command.CustomerIdentifier)
        {
            throw new OrderNotFoundException(command.OrderIdentifier);
        }
        
        order.SubmitOrder();

        await orderRepository.Update(order);

        return new OrderDto(order);
    }
}