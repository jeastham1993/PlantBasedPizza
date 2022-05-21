using Amazon.Lambda.CloudWatchEvents;
using Amazon.Lambda.CloudWatchEvents.ECSEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entites;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.OrderManager.Serverless;

public class OrderQualityCheckedEventHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IObservabilityService _logger;
    
    public OrderQualityCheckedEventHandler()
    {
        Startup.Configure();

        this._orderRepository = Startup.Services.GetRequiredService<IOrderRepository>();
        this._logger = Startup.Services.GetRequiredService<IObservabilityService>();
    }

    public async void FunctionHandler(CloudWatchEvent<OrderQualityCheckedEvent> evt)
    {
        var order = await this._orderRepository.Retrieve(evt.Detail.OrderIdentifier);

        order.AddHistory("Order quality checked");

        if (order.OrderType == OrderType.DELIVERY)
        {
            order.AddHistory("Sending for delivery");

            await EventManager.Raise(new OrderReadyForDeliveryEvent(order.OrderIdentifier,
                order.DeliveryDetails.AddressLine1, order.DeliveryDetails.AddressLine2,
                order.DeliveryDetails.AddressLine3, order.DeliveryDetails.AddressLine4,
                order.DeliveryDetails.AddressLine5, order.DeliveryDetails.Postcode));
        }
        else
        {
            order.IsAwaitingCollection();
        }

        await this._orderRepository.Update(order).ConfigureAwait(false);
    }
}
