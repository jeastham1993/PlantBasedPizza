using Amazon.Lambda.CloudWatchEvents;
using Amazon.Lambda.CloudWatchEvents.ECSEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entites;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.OrderManager.Serverless;

public class DriverDeliveredOrderEventHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IObservabilityService _observabilityService;
    
    public DriverDeliveredOrderEventHandler()
    {
        Startup.Configure();

        this._orderRepository = Startup.Services.GetRequiredService<IOrderRepository>();
        this._observabilityService = Startup.Services.GetRequiredService<IObservabilityService>();
    }

    public async void FunctionHandler(CloudWatchEvent<OrderDeliveredEvent> evt)
    {
        this._observabilityService.Info($"Processing an Order delivered event for {evt.Detail.OrderIdentifier}");
            
        var order = await this._orderRepository.Retrieve(evt.Detail.OrderIdentifier);

        this._observabilityService.Info("Found order");

        order.CompleteOrder();

        this._observabilityService.Info("Order marked as completed");
            
        await this._orderRepository.Update(order).ConfigureAwait(false);

        this._observabilityService.Info("Updated!");
    }
}
