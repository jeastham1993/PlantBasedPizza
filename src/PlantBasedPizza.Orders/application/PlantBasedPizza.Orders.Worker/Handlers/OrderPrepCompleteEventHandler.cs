using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;

namespace PlantBasedPizza.Orders.Worker.Handlers
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
        
        public async Task Handle(OrderPrepCompleteEventV1 evt)
        {
            var order = await _orderRepository.Retrieve(evt.OrderIdentifier);
            
            order.AddHistory("Order prep completed");
            
            await _orderRepository.Update(order);
            
            await _userNotificationService.NotifyOrderPrepComplete(order.CustomerIdentifier, order.OrderIdentifier);
        }
    }
}