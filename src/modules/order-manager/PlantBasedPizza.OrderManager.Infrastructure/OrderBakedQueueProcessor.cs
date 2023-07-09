using Microsoft.Extensions.Hosting;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Handlers;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class OrderBakedQueueProcessor : BackgroundService
{
    private readonly InboundEventQueueProcessor<OrderBakedEvent, OrderBakedEventHandler> _processor;

    public OrderBakedQueueProcessor(
        InboundEventQueueProcessor<OrderBakedEvent, OrderBakedEventHandler> processor)
    {
        _processor = processor;
    }
    
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await this._processor.ProcessAsync(InfrastructureConstants.OrderBakedQueueUrl, stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}