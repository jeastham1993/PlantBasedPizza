using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.OrderManager.Core.CreateDeliveryOrder;

public class CreateDeliveryOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderFactory _orderFactory;

    public CreateDeliveryOrderCommandHandler(IOrderRepository orderRepository, IOrderFactory orderFactory)
    {
        _orderRepository = orderRepository;
        _orderFactory = orderFactory;
    }

    public async Task<OrderDto?> Handle(CreateDeliveryOrderCommand request)
    {
        var order = await _orderFactory.CreateAsync(request.OrderType, request.CustomerIdentifier,
            new DeliveryDetails
            {
                AddressLine1 = request.AddressLine1,
                AddressLine2 = request.AddressLine2,
                AddressLine3 = request.AddressLine3,
                AddressLine4 = request.AddressLine4,
                AddressLine5 = request.AddressLine5,
                Postcode = request.Postcode
            }, CorrelationContext.GetCorrelationId());

        await _orderRepository.Add(order);

        return new OrderDto(order);
    }
}