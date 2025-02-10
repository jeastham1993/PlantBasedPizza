using Microsoft.Extensions.Logging;
using PlantBasedPizza.OrderManager.Core.ExternalEvents;
using PlantBasedPizza.OrderManager.Core.Services;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Core.OrderPreparing
{
    public class OrderPreparingEventHandler(
        IOrderRepository orderRepository,
        ILogger<OrderPreparingEventHandler> logger,
        IUserNotificationService userNotificationService)
    {
        [Channel("kitchen.orderPreparing.v1")]
        [PublishOperation(typeof(OrderPreparingEventV1), OperationId = nameof(OrderPreparingEventV1))]
        public async Task Handle(OrderPreparing evt)
        {
            logger.LogInformation($"[ORDER-MANAGER] Handling order preparing event");
            
            var order = await orderRepository.Retrieve(evt.OrderIdentifier);

            order.AddHistory("Order prep started");

            await orderRepository.Update(order);
            await userNotificationService.NotifyOrderPreparing(order.CustomerIdentifier, order.OrderIdentifier);
        }
    }
}