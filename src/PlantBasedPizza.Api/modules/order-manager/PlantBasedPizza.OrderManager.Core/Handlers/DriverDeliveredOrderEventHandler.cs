using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Shared.Events;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Core.Handlers
{
    [AsyncApi]
    public class DriverDeliveredOrderEventHandler : Handles<OrderDeliveredEvent>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderDomainService _orderDomainService;

        public DriverDeliveredOrderEventHandler(IOrderRepository orderRepository, IOrderDomainService orderDomainService)
        {
            _orderRepository = orderRepository;
            _orderDomainService = orderDomainService;
        }

        [Channel("delivery.order-delivered")] // Creates a Channel
        [SubscribeOperation(typeof(OrderDeliveredEvent), Summary = "Handle an order delivered event.", OperationId = "delivery.order-delivered")]
        public async Task Handle(OrderDeliveredEvent evt)
        {
            var order = await _orderRepository.Retrieve(evt.OrderIdentifier);

            await _orderDomainService.CompleteOrderAsync(order);
            
            await _orderRepository.Update(order).ConfigureAwait(false);
        }
    }
}