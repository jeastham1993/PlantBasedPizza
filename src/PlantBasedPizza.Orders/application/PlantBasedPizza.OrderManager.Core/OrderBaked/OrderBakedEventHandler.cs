using PlantBasedPizza.OrderManager.Core.ExternalEvents;
using PlantBasedPizza.OrderManager.Core.Services;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Core.OrderBaked
{
    public class OrderBakedEventHandler(
        IOrderRepository orderRepository,
        IUserNotificationService userNotificationService)
    {
        [Channel("kitchen.orderBaked.v1")]
        [PublishOperation(typeof(OrderBakedEventV1), OperationId = nameof(OrderBakedEventV1))]
        public async Task Handle(OrderBaked evt)
        {
            var order = await orderRepository.Retrieve(evt.OrderIdentifier);

            order.AddHistory("Order baked");

            await orderRepository.Update(order);
            await userNotificationService.NotifyOrderBakeComplete(order.CustomerIdentifier, order.OrderIdentifier);
        }
    }
}