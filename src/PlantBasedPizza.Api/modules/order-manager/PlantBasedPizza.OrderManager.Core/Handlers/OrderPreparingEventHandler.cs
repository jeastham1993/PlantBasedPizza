using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.Shared.Events;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Core.Handlers
{
    [AsyncApi]
    public class OrderPreparingEventHandler : Handles<OrderPreparingEvent>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderPreparingEventHandler> _logger;

        public OrderPreparingEventHandler(IOrderRepository orderRepository, ILogger<OrderPreparingEventHandler> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        [Channel("kitchen.prep-started")] // Creates a Channel
        [SubscribeOperation(typeof(OrderPreparingEvent), Summary = "Handle an order prep started event.", OperationId = "kitchen.prep-started")]
        public async Task Handle(OrderPreparingEvent evt)
        {
            this._logger.LogInformation($"[ORDER-MANAGER] Handling order preparing event");
            
            var order = await this._orderRepository.Retrieve(evt.OrderIdentifier);

            order.AddHistory("Order prep started");

            await this._orderRepository.Update(order);
        }
    }
}