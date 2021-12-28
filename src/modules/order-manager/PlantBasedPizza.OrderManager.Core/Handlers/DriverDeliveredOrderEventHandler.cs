using System.Threading.Tasks;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entites;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Core.Handlers
{
    [AsyncApi]
    public class DriverDeliveredOrderEventHandler : Handles<OrderDeliveredEvent>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IObservabilityService _observabilityService;

        public DriverDeliveredOrderEventHandler(IOrderRepository orderRepository, IObservabilityService observabilityService)
        {
            _orderRepository = orderRepository;
            _observabilityService = observabilityService;
        }

        [Channel("delivery.order-delivered")] // Creates a Channel
        [SubscribeOperation(typeof(OrderDeliveredEvent), Summary = "Handle an order delivered event.", OperationId = "delivery.order-delivered")]
        public async Task Handle(OrderDeliveredEvent evt)
        {
            this._observabilityService.Info($"Processing an Order delivered event for {evt.OrderIdentifier}");
            
            var order = await this._orderRepository.Retrieve(evt.OrderIdentifier);

            this._observabilityService.Info("Found order");

            order.CompleteOrder();

            this._observabilityService.Info("Order marked as completed");
            
            await this._orderRepository.Update(order).ConfigureAwait(false);

            this._observabilityService.Info("Updated!");
        }
    }
}