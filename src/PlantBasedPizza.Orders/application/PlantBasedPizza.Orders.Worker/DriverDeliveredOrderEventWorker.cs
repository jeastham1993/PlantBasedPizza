using System.Diagnostics;
using Microsoft.Extensions.Options;
using PlantBasedPizza.Events;
using PlantBasedPizza.Orders.Worker.Handlers;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;

namespace PlantBasedPizza.Orders.Worker;

public class DriverDeliveredOrderEventWorker : BackgroundService
{
    private readonly SqsEventSubscriber _eventSubscriber;
    private readonly ActivitySource _source;
    private readonly DriverDeliveredOrderEventHandler _eventHandler;
    private readonly QueueConfiguration _queueConfiguration;

    public DriverDeliveredOrderEventWorker(SqsEventSubscriber eventSubscriber, ActivitySource source,
        DriverDeliveredOrderEventHandler eventHandler, IOptions<QueueConfiguration> queueConfiguration)
    {
        _eventSubscriber = eventSubscriber;
        _source = source;
        _eventHandler = eventHandler;
        _queueConfiguration = queueConfiguration.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueUrl = await this._eventSubscriber.GetQueueUrl(_queueConfiguration.DriverDeliveredOrderQueue);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = await _eventSubscriber.GetMessages<DriverDeliveredOrderEventV1>(queueUrl);

            foreach (var message in messages)
            {
                using var processingActivity = _source.StartActivity("processing-driver-collected-order-event",
                    ActivityKind.Server, message.TraceParent);
                processingActivity.AddTag("queue.time", message.QueueTime);

                processingActivity.SetTag("orderIdentifier", message.EventData.OrderIdentifier);

                await _eventHandler.Handle(message.EventData);

                await _eventSubscriber.Ack(queueUrl, message);
            }
            
            await Task.Delay(1000, stoppingToken);
        }
    }
}