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
            this._logger.LogInformation("[ORDER-MANAGER] Handling order prep complete event");
            
            var order = await this._orderRepository.Retrieve(evt.OrderIdentifier);
            
            this._logger.LogInformation("[ORDER-MANAGER] Found order");

            order.AddHistory("Order prep completed");
            
            this._logger.LogInformation("[ORDER-MANAGER] Added history");

            await this._orderRepository.Update(order);
            
            this._logger.LogInformation("[ORDER-MANAGER] Wrote updates to database");
        }
    }
}