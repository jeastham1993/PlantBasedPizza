using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.OrderReadyForDelivery;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;

namespace PlantBasedPizza.Orders.Worker.Handlers
{
    public class OrderQualityCheckedEventHandler(
        OrderReadyForDeliveryCommandHandler commandHandler,
        IFeatures features,
        IWorkflowEngine workflowEngine)
    {
        public async Task Handle(OrderQualityCheckedEventV1 evt)
        {
            if (features.UseOrchestrator())
            {
                await workflowEngine.OrderReadyForDelivery(evt.OrderIdentifier);
                return;
            }

            await commandHandler.Handle(new OrderReadyForDeliveryCommand()
            {
                OrderIdentifier = evt.OrderIdentifier
            });
        }
    }
}