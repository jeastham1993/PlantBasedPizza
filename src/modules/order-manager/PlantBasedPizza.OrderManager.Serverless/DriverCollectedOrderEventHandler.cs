using Amazon.Lambda.CloudWatchEvents;
using Amazon.Lambda.CloudWatchEvents.ECSEvents;
using Amazon.Lambda.CloudWatchEvents.ScheduledEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entites;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.OrderManager.Serverless;

public class DriverCollectedOrderEventHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IObservabilityService _logger;
    
    public DriverCollectedOrderEventHandler()
    {
        Startup.Configure();

        this._orderRepository = Startup.Services.GetRequiredService<IOrderRepository>();
        this._logger = Startup.Services.GetRequiredService<IObservabilityService>();
    }

    public async void FunctionHandler(CloudWatchEvent<DriverCollectedOrderEvent> evt)
    {
        var order = await this._orderRepository.Retrieve(evt.Detail.OrderIdentifier);

        order.AddHistory($"Order collected by driver {evt.Detail.DriverName}");
            
        await this._orderRepository.Update(order).ConfigureAwait(false);
    }
}
