using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.OrderManager.Core.CreatePickupOrder;

public class CreatePickupOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderFactory _orderFactory;

    public CreatePickupOrderCommandHandler(IOrderRepository orderRepository, IOrderFactory orderFactory)
    {
        _orderRepository = orderRepository;
        _orderFactory = orderFactory;
    }

    public async Task<OrderDto?> Handle(CreatePickupOrderCommand request)
    {
        var order = await _orderFactory.CreateAsync(request.OrderType, request.CustomerIdentifier, null, CorrelationContext.GetCorrelationId());

        await this._orderRepository.Add(order);

        return new OrderDto(order);
    }
}