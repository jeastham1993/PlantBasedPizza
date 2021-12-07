using System.Threading.Tasks;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entites;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.OrderManager.Core.Handlers
{
    public class DriverCollectedOrderEventHandler : Handles<DriverCollectedOrderEvent>
    {
        private readonly IOrderRepository _orderRepository;

        public DriverCollectedOrderEventHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task Handle(DriverCollectedOrderEvent evt)
        {
            var order = await this._orderRepository.Retrieve(evt.OrderIdentifier);

            order.AddHistory($"Order collected by driver {evt.DriverName}");
            
            await this._orderRepository.Update(order).ConfigureAwait(false);
        }
    }
}