using System.Diagnostics;
using Microsoft.Extensions.Options;
using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.Worker.Handlers;
using PlantBasedPizza.Kitchen.Worker.IntegrationEvents;

namespace PlantBasedPizza.Kitchen.Worker;

public class OrderSubmittedEventWorker : BackgroundService
{
    private readonly SqsEventSubscriber _eventSubscriber;
    private readonly ActivitySource _source;
    private readonly OrderSubmittedEventHandler _eventHandler;
    private readonly QueueConfiguration _queueConfiguration;
    private readonly ILogger<OrderSubmittedEventWorker> _logger;

    public OrderSubmittedEventWorker(SqsEventSubscriber eventSubscriber, ActivitySource source,
        OrderSubmittedEventHandler eventHandler, IOptions<QueueConfiguration> queueConfiguration, ILogger<OrderSubmittedEventWorker> logger)
    {
        _eventSubscriber = eventSubscriber;
        _source = source;
        _eventHandler = eventHandler;
        _logger = logger;
        _queueConfiguration = queueConfiguration.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this._logger.LogInformation("Starting worker");

        try
        {
            var queueUrl = await this._eventSubscriber.GetQueueUrl(_queueConfiguration.OrderSubmittedQueue);
        
            while (!stoppingToken.IsCancellationRequested)
            {
                var messages = await _eventSubscriber.GetMessages<OrderSubmittedEventV1>(queueUrl);

                foreach (var message in messages)
                {
                    using var processingActivity = _source.StartActivity("processing-order-submitted-event-event",
                        ActivityKind.Server, message.TraceParent);
                
                    processingActivity.AddTag("queue.time", message.QueueTime);

                    processingActivity.SetTag("orderIdentifier", message.EventData.OrderIdentifier);

                    await _eventHandler.Handle(message.EventData);

                    await _eventSubscriber.Ack(queueUrl, message);
                }
            
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (Exception e)
        {
            this._logger.LogError(e, "Failure starting service");
            throw;
        }
    }
}