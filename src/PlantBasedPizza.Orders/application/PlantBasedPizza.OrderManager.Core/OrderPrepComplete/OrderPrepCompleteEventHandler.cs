using Microsoft.Extensions.Logging;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Core.OrderPrepComplete
{
    public class OrderPrepCompleteEventHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserNotificationService _userNotificationService;

        public OrderPrepCompleteEventHandler(IOrderRepository orderRepository, ILogger<OrderPrepCompleteEventHandler> logger, IUserNotificationService userNotificationService)
        {
            _orderRepository = orderRepository;
            _userNotificationService = userNotificationService;
        }
        
        [Channel("kitchen.orderPrepComplete.v1")]
        [PublishOperation(typeof(OrderPrepCompleteEventV1), OperationId = nameof(OrderPrepCompleteEventV1))]
        public async Task Handle(OrderPrepCompleteEventV1 evt)
        {
            var order = await _orderRepository.Retrieve(evt.OrderIdentifier);
            
            order.AddHistory("Order prep completed");
            
            await _orderRepository.Update(order);
            
            await _userNotificationService.NotifyOrderPrepComplete(order.CustomerIdentifier, order.OrderIdentifier);
        }
    }
}