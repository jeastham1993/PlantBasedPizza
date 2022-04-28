using Amazon.Lambda.CloudWatchEvents;
using Amazon.Lambda.CloudWatchEvents.ECSEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entites;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.OrderManager.Serverless;

public class OrderPreparingEventHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IObservabilityService _logger;
    
    public OrderPreparingEventHandler()
    {
        Startup.Configure();

        this._orderRepository = Startup.Services.GetRequiredService<IOrderRepository>();
        this._logger = Startup.Services.GetRequiredService<IObservabilityService>();
    }

    public async void FunctionHandler(CloudWatchEvent<OrderPreparingEvent> evt)
    {
        this._logger.Info($"[ORDER-MANAGER] Handling order preparing event");
            
        var order = await this._orderRepository.Retrieve(evt.Detail.OrderIdentifier);

        order.AddHistory("Order prep started");

        await this._orderRepository.Update(order);
    }
}
