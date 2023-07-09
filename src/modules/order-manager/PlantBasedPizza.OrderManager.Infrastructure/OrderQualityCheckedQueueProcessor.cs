using Microsoft.Extensions.Hosting;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Handlers;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class OrderQualityCheckedQueueProcessor : BackgroundService
{
    private readonly InboundEventQueueProcessor<OrderQualityCheckedEvent, OrderQualityCheckedEventHandler> _processor;

    public OrderQualityCheckedQueueProcessor(
        InboundEventQueueProcessor<OrderQualityCheckedEvent, OrderQualityCheckedEventHandler> processor)
    {
        _processor = processor;
    }
    
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await this._processor.ProcessAsync(InfrastructureConstants.OrderQualityCheckedQueueUrl, stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}