using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.Shared.Logging;

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
        var order = Order.Create(request.OrderType, request.CustomerIdentifier);

        await this._orderRepository.Add(order);

        return new OrderDto(order);
    }
}