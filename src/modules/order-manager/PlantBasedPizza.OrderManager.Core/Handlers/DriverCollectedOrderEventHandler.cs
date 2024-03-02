using System.Threading.Tasks;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.Shared.Events;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Core.Handlers
{
    [AsyncApi("OrderManager")]
    public class DriverCollectedOrderEventHandler : Handles<DriverCollectedOrderEvent>
    {
        private readonly IOrderRepository _orderRepository;

        public DriverCollectedOrderEventHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [Channel("delivery.driver-collected")] // Creates a Channel
        [SubscribeOperation(typeof(DriverCollectedOrderEvent), Summary = "Handle a driver order collected event.", OperationId = "delivery.driver-collected")]
        public async Task Handle(DriverCollectedOrderEvent evt)
        {
            var order = await this._orderRepository.Retrieve(evt.OrderIdentifier);

            order.AddHistory($"Order collected by driver {evt.DriverName}");
            
            await this._orderRepository.Update(order).ConfigureAwait(false);
        }
    }
}