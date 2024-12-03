using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Core.CancelOrder;

public class CancelOrderCommandHandler(IOrderRepository orderRepository, IUserNotificationService userNotificationService)
{
    public async Task<CancelOrderResult> Handle(CancelOrderCommand command)
    {
        var order = await orderRepository.Retrieve(command.OrderIdentifier);

        var cancelResult = order.CancelOrder();

        if (!cancelResult)
        {
            await userNotificationService.NotifyCancellationFailed(order.CustomerIdentifier, order.OrderIdentifier);
            return new CancelOrderResult()
            {
                CancelSuccess = false,
            };
        }

        await orderRepository.Update(order);

        await userNotificationService.NotifyOrderCancelled(order.CustomerIdentifier, order.OrderIdentifier);
        
        return new CancelOrderResult()
        {
            CancelSuccess = cancelResult
        };
    }
}