using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.OrderManager.Core.CreatePickupOrder;

public class CreatePickupOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;

    public CreatePickupOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto?> Handle(CreatePickupOrderCommand request)
    {
        if (await _orderRepository.Exists(request.OrderIdentifier))
        {
            return null;
        }
            
        var order = Order.Create(request.OrderIdentifier, request.OrderType, request.CustomerIdentifier, null);

        await _orderRepository.Add(order);

        return new OrderDto(order);
    }
}