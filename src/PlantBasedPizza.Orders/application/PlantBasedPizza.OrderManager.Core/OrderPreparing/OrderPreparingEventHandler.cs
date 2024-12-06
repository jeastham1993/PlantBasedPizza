using Microsoft.Extensions.Logging;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Core.OrderPreparing
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
        
        [Channel("kitchen.orderPreparing.v1")]
        [PublishOperation(typeof(OrderPreparingEventV1), OperationId = nameof(OrderPreparingEventV1))]
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