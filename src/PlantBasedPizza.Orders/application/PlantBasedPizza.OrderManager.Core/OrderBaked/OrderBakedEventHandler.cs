using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Core.OrderBaked
{
    public class OrderBakedEventHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserNotificationService _userNotificationService;

        public OrderBakedEventHandler(IOrderRepository orderRepository, IUserNotificationService userNotificationService)
        {
            _orderRepository = orderRepository;
            _userNotificationService = userNotificationService;
        }
        
        [Channel("kitchen.orderBaked.v1")]
        [PublishOperation(typeof(OrderBakedEventV1), OperationId = nameof(OrderBakedEventV1))]
        public async Task Handle(OrderBakedEventV1 evt)
        {
            var order = await _orderRepository.Retrieve(evt.OrderIdentifier);

            order.AddHistory("Order baked");

            await _orderRepository.Update(order);
            await _userNotificationService.NotifyOrderBakeComplete(order.CustomerIdentifier, order.OrderIdentifier);
        }
    }
}