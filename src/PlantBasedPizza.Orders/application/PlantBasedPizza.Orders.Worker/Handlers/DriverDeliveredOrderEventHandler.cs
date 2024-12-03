using PlantBasedPizza.OrderManager.Core.OrderDelivered;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;

namespace PlantBasedPizza.Orders.Worker.Handlers;

public class DriverDeliveredOrderEventHandler(
    OrderDeliveredEventHandler eventHandler,
    IFeatures features,
    IWorkflowEngine workflowEngine)
{
    public async Task Handle(DriverDeliveredOrderEventV1 evt)
    {
        if (features.UseOrchestrator())
        {
            await workflowEngine.OrderDelivered(evt.OrderIdentifier);
            return;
        }

        await eventHandler.Handle(new OrderDeliveredEvent
        {
            OrderIdentifier = evt.OrderIdentifier
        });
    }
}