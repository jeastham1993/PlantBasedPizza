using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.OrderManager.Core.CreateDeliveryOrder;

public class CreateDeliveryOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;

    public CreateDeliveryOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto?> Handle(CreateDeliveryOrder request)
    {
        var order = Order.Create(request.OrderType, request.CustomerIdentifier,
            new DeliveryDetails
            {
                AddressLine1 = request.AddressLine1,
                AddressLine2 = request.AddressLine2,
                AddressLine3 = request.AddressLine3,
                AddressLine4 = request.AddressLine4,
                AddressLine5 = request.AddressLine5,
                Postcode = request.Postcode
            });

        await _orderRepository.Add(order);

        return new OrderDto(order);
    }
}