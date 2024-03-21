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
        private readonly ILoyaltyPointService _loyaltyPointService;

        public DriverDeliveredOrderEventHandler(IOrderRepository orderRepository, ILoyaltyPointService loyaltyPointService)
        {
            _orderRepository = orderRepository;
            _loyaltyPointService = loyaltyPointService;
        }

        [Channel("delivery.order-delivered")] // Creates a Channel
        [SubscribeOperation(typeof(OrderDeliveredEvent), Summary = "Handle an order delivered event.", OperationId = "delivery.order-delivered")]
        public async Task Handle(OrderDeliveredEvent evt)
        {
            var order = await this._orderRepository.Retrieve(evt.OrderIdentifier);

            order.CompleteOrder();
            
            await this._orderRepository.Update(order).ConfigureAwait(false);
            await this._loyaltyPointService.AddLoyaltyPoints(order.CustomerIdentifier, evt.OrderIdentifier, order.TotalPrice);
        }
    }
}