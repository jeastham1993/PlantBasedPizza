using Microsoft.Extensions.Logging;
using PlantBasedPizza.OrderManager.Core.ExternalEvents;
using PlantBasedPizza.OrderManager.Core.Services;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Core.OrderPrepComplete
{
    public class OrderPrepCompleteEventHandler(
        IOrderRepository orderRepository,
        IUserNotificationService userNotificationService)
    {
        [Channel("kitchen.orderPrepComplete.v1")]
        [PublishOperation(typeof(OrderPrepCompleteEventV1), OperationId = nameof(OrderPrepCompleteEventV1))]
        public async Task Handle(OrderPrepComplete evt)
        {
            var order = await orderRepository.Retrieve(evt.OrderIdentifier);
            
            order.AddHistory("Order prep completed");
            
            await orderRepository.Update(order);
            
            await userNotificationService.NotifyOrderPrepComplete(order.CustomerIdentifier, order.OrderIdentifier);
        }
    }
}