using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Core.OrderReadyForDelivery;

public class OrderReadyForDeliveryCommandHandler(
    IOrderRepository orderRepository,
    IUserNotificationService notificationService)
{
    public async Task<OrderDto?> Handle(OrderReadyForDeliveryCommand command)
    {
        var order = await orderRepository.Retrieve(command.OrderIdentifier);

        order.AddHistory("Order quality checked");

        await notificationService.NotifyOrderQualityCheckComplete(order.CustomerIdentifier, order.OrderIdentifier);

        if (order.OrderType == OrderType.Delivery)
        {
            order.ReadyForDelivery();
        }
        else
        {
            order.IsAwaitingCollection();
            await notificationService.NotifyReadyForCollection(order.CustomerIdentifier, order.OrderIdentifier);
        }

        await orderRepository.Update(order);

        return new OrderDto(order);
    }
}