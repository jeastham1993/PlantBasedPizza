using PlantBasedPizza.OrderManager.Core.OrderReadyForDelivery;
using PlantBasedPizza.OrderManager.Core.Services;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Core.OrderQualityChecked
{
    public class OrderQualityCheckedEventHandler(
        OrderReadyForDeliveryCommandHandler commandHandler,
        IFeatures features,
        IWorkflowEngine workflowEngine)
    {
        [Channel("kitchen.qualityChecked.v1")]
        [PublishOperation(typeof(OrderQualityCheckedEventV1), OperationId = nameof(OrderQualityCheckedEventV1))]
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