using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.OrderManager.Core.CreatePickupOrder;

public class CreatePickupOrderCommandHandler(IOrderRepository orderRepository)
{
    public async Task<OrderDto?> Handle(CreatePickupOrderCommand request)
    {
        var order = Order.Create(CreatePickupOrderCommand.OrderType, request.CustomerIdentifier);

        await orderRepository.Add(order);

        return new OrderDto(order);
    }
}