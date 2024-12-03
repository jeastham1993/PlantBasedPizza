using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;

namespace PlantBasedPizza.Orders.Worker.Handlers
{
    public class OrderPreparingEventHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderPreparingEventHandler> _logger;
        private readonly IUserNotificationService _userNotificationService;

        public OrderPreparingEventHandler(IOrderRepository orderRepository, ILogger<OrderPreparingEventHandler> logger, IUserNotificationService userNotificationService)
        {
            _orderRepository = orderRepository;
            _logger = logger;
            _userNotificationService = userNotificationService;
        }
        
        public async Task Handle(OrderPreparingEventV1 evt)
        {
            _logger.LogInformation($"[ORDER-MANAGER] Handling order preparing event");
            
            var order = await _orderRepository.Retrieve(evt.OrderIdentifier);

            order.AddHistory("Order prep started");

            await _orderRepository.Update(order);
            await _userNotificationService.NotifyOrderPreparing(order.CustomerIdentifier, order.OrderIdentifier);
        }
    }
}