using Microsoft.Extensions.Hosting;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Handlers;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class DriverCollectedOrderQueueProcessor : BackgroundService
{
    private readonly InboundEventQueueProcessor<DriverCollectedOrderEvent, DriverCollectedOrderEventHandler> _processor;

    public DriverCollectedOrderQueueProcessor(
        InboundEventQueueProcessor<DriverCollectedOrderEvent, DriverCollectedOrderEventHandler> processor)
    {
        _processor = processor;
    }
    
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await this._processor.ProcessAsync(InfrastructureConstants.DriverCollectedQueueUrl, stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}