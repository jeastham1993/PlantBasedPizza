using Microsoft.Extensions.Hosting;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Handlers;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class OrderDeliveredQueueProcessor : BackgroundService
{
    private readonly InboundEventQueueProcessor<OrderDeliveredEvent, DriverDeliveredOrderEventHandler> _processor;

    public OrderDeliveredQueueProcessor(
        InboundEventQueueProcessor<OrderDeliveredEvent, DriverDeliveredOrderEventHandler> processor)
    {
        _processor = processor;
    }
    
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await this._processor.ProcessAsync(InfrastructureConstants.OrderDeliveredQueueUrl, stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}