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
        if (await this._orderRepository.Exists(request.OrderIdentifier))
        {
            return null;
        }
            
        var order = Order.Create(request.OrderIdentifier, request.OrderType, request.CustomerIdentifier, null, CorrelationContext.GetCorrelationId());

        await this._orderRepository.Add(order);

        return new OrderDto(order);
    }
}