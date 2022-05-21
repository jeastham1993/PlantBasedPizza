using Amazon.Lambda.CloudWatchEvents;
using Amazon.Lambda.CloudWatchEvents.ECSEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entites;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.OrderManager.Serverless;

public class OrderPrepCompleteEventHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IObservabilityService _logger;
    
    public OrderPrepCompleteEventHandler()
    {
        Startup.Configure();

        this._orderRepository = Startup.Services.GetRequiredService<IOrderRepository>();
        this._logger = Startup.Services.GetRequiredService<IObservabilityService>();
    }

    public async void FunctionHandler(CloudWatchEvent<OrderPrepCompleteEvent> evt)
    {
        this._logger.Info("[ORDER-MANAGER] Handling order prep complete event");
            
        var order = await this._orderRepository.Retrieve(evt.Detail.OrderIdentifier);
            
        this._logger.Info("[ORDER-MANAGER] Found order");

        order.AddHistory("Order prep completed");
            
        this._logger.Info("[ORDER-MANAGER] Added history");

        await this._orderRepository.Update(order);
            
        this._logger.Info("[ORDER-MANAGER] Wrote updates to database");
    }
}
