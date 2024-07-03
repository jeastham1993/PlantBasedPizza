using BackgroundWorkers.IntegrationEvents;
using Datadog.Trace;
using PlantBasedPizza.OrderManager.Core.Entities;

namespace BackgroundWorkers.Handlers
{
    public class OrderPreparingEventHandler
    {
        private readonly IOrderRepository _orderRepository;

        public OrderPreparingEventHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        
        public async Task Handle(OrderPreparingEventV1 evt)
        {
            Tracer.Instance.ActiveScope.Span.SetTag("order.orderIdentifier", evt.OrderIdentifier);
            Tracer.Instance.ActiveScope.Span.SetTag("order.kitchenIdentifier", evt.KitchenIdentifier);

            var order = await this._orderRepository.RetrieveByOrderId(evt.OrderIdentifier);

            order.AddHistory("Order prep started");

            await this._orderRepository.Update(order);
        }
    }
}