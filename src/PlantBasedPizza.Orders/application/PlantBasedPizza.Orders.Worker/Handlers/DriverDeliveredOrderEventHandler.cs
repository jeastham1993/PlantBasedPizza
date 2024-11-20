using Dapr.Client;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Infrastructure.IntegrationEvents;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;

namespace PlantBasedPizza.Orders.Worker.Handlers
{
    public class DriverDeliveredOrderEventHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly DaprClient _daprClient;

        public DriverDeliveredOrderEventHandler(IOrderRepository orderRepository, DaprClient daprClient)
        {
            _orderRepository = orderRepository;
            _daprClient = daprClient;
        }

        public async Task Handle(DriverDeliveredOrderEventV1 evt)
        {
            var order = await this._orderRepository.Retrieve(evt.OrderIdentifier);

            order.CompleteOrder();
            
            await this._orderRepository.Update(order).ConfigureAwait(false);

            var completedEvt = new OrderCompletedIntegrationEventV1()
            {
                OrderIdentifier = order.OrderIdentifier,
                CustomerIdentifier = order.CustomerIdentifier,
                OrderValue = order.TotalPrice
            };
            await this._daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", completedEvt);
        }
    }
}