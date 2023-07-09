using Microsoft.Extensions.Hosting;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Handlers;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class OrderPreparingQueueProcessor : BackgroundService
{
    private readonly InboundEventQueueProcessor<OrderPreparingEvent, OrderPreparingEventHandler> _processor;

    public OrderPreparingQueueProcessor(
        InboundEventQueueProcessor<OrderPreparingEvent, OrderPreparingEventHandler> processor)
    {
        _processor = processor;
    }
    
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await this._processor.ProcessAsync(InfrastructureConstants.OrderPreparingQueueUrl, stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}