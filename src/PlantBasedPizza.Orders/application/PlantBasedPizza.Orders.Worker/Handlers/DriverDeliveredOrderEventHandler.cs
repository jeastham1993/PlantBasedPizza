using PlantBasedPizza.OrderManager.Core.OrderDelivered;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;
using Saunter.Attributes;

namespace PlantBasedPizza.Orders.Worker.Handlers;

public class DriverDeliveredOrderEventHandler(
    OrderDeliveredEventHandler eventHandler,
    IFeatures features,
    IWorkflowEngine workflowEngine)
{
    [Channel("delivery.driverDeliveredOrder.v1")]
    [PublishOperation(typeof(DriverDeliveredOrderEventV1), OperationId = nameof(DriverDeliveredOrderEventV1))]
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