using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Core.KitchenConfirmedOrder;

public class KitchenConfirmedOrderEventHandler(
    IOrderRepository orderRepository,
    IFeatures features,
    IWorkflowEngine workflowEngine,
    IUserNotificationService notificationService)
{
    [Channel("kitchen.orderConfirmed.v1")]
    [PublishOperation(typeof(KitchenConfirmedOrderEventV1), OperationId = nameof(KitchenConfirmedOrderEventV1))]
    public async Task Handle(KitchenConfirmedOrderEventV1 evt)
    {
        var order = await orderRepository.Retrieve(evt.OrderIdentifier);
            
        await notificationService.NotifyKitchenReceipt(order.CustomerIdentifier, evt.OrderIdentifier);
        if (features.UseOrchestrator())
        {
            await workflowEngine.ConfirmKitchenReceipt(evt.OrderIdentifier);
            return;
        }
    }
}