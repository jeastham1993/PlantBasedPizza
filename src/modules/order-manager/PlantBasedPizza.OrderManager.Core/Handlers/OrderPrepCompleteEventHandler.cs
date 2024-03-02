using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.Shared.Events;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Core.Handlers
{
    [AsyncApi]
    public class OrderPrepCompleteEventHandler : Handles<OrderPrepCompleteEvent>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderPrepCompleteEventHandler> _logger;

        public OrderPrepCompleteEventHandler(IOrderRepository orderRepository, ILogger<OrderPrepCompleteEventHandler> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        [Channel("kitchen.prep-complete")] // Creates a Channel
        [SubscribeOperation(typeof(OrderPrepCompleteEvent), Summary = "Handle an order prep completed event.", OperationId = "kitchen.prep-complete")]
        public async Task Handle(OrderPrepCompleteEvent evt)
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