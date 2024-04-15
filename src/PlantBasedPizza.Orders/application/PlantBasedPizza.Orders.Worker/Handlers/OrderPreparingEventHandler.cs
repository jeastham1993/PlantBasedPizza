using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;

namespace PlantBasedPizza.Orders.Worker.Handlers
{
    public class OrderPreparingEventHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderPreparingEventHandler> _logger;

        public OrderPreparingEventHandler(IOrderRepository orderRepository, ILogger<OrderPreparingEventHandler> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }
        
        public async Task Handle(OrderPreparingEventV1 evt)
        {
            this._logger.LogInformation($"[ORDER-MANAGER] Handling order preparing event");
            
            var order = await this._orderRepository.Retrieve(evt.OrderIdentifier);

            order.AddHistory("Order prep started");

            await this._orderRepository.Update(order);
        }
    }
}