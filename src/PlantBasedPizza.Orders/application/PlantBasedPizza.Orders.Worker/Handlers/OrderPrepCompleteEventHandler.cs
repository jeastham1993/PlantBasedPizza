using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;

namespace PlantBasedPizza.Orders.Worker.Handlers
{
    public class OrderPrepCompleteEventHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderPrepCompleteEventHandler> _logger;

        public OrderPrepCompleteEventHandler(IOrderRepository orderRepository, ILogger<OrderPrepCompleteEventHandler> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }
        
        public async Task Handle(OrderPrepCompleteEventV1 evt)
        {
            _logger.LogInformation("[ORDER-MANAGER] Handling order prep complete event");
            
            var order = await _orderRepository.Retrieve(evt.OrderIdentifier);
            
            _logger.LogInformation("[ORDER-MANAGER] Found order");

            order.AddHistory("Order prep completed");
            
            _logger.LogInformation("[ORDER-MANAGER] Added history");

            await _orderRepository.Update(order);
            
            _logger.LogInformation("[ORDER-MANAGER] Wrote updates to database");
        }
    }
}