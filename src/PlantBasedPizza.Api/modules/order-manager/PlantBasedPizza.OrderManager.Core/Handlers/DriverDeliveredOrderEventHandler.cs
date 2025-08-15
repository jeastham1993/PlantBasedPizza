using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.CompleteOrder;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Shared.Events;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Core.Handlers
{
    [AsyncApi]
    public class DriverDeliveredOrderEventHandler : Handles<OrderDeliveredEvent>
    {
        private readonly CompleteOrderCommandHandler _completeOrderCommandHandler;

        public DriverDeliveredOrderEventHandler(CompleteOrderCommandHandler completeOrderCommandHandler)
        {
            _completeOrderCommandHandler = completeOrderCommandHandler;
        }

        [Channel("delivery.order-delivered")] // Creates a Channel
        [SubscribeOperation(typeof(OrderDeliveredEvent), Summary = "Handle an order delivered event.", OperationId = "delivery.order-delivered")]
        public async Task Handle(OrderDeliveredEvent evt)
        {
            await _completeOrderCommandHandler.Handle(new CompleteOrderCommand 
            { 
                OrderIdentifier = evt.OrderIdentifier,
                CorrelationId = evt.CorrelationId
            });
        }
    }
}