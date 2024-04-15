using System.Diagnostics;
using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.Worker.Handlers;
using PlantBasedPizza.Kitchen.Worker.IntegrationEvents;
using RabbitMQ.Client;

namespace PlantBasedPizza.Kitchen.Worker;

public class OrderSubmittedEventWorker : BackgroundService
{
    private readonly RabbitMqEventSubscriber _eventSubscriber;
    private readonly ActivitySource _source;
    private readonly OrderSubmittedEventHandler _eventHandler;
    private readonly ILogger<OrderSubmittedEventWorker> _logger;

    public OrderSubmittedEventWorker(RabbitMqEventSubscriber eventSubscriber, ActivitySource source, OrderSubmittedEventHandler eventHandler, ILogger<OrderSubmittedEventWorker> logger)
    {
        _eventSubscriber = eventSubscriber;
        _source = source;
        _eventHandler = eventHandler;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this._logger.LogInformation("Starting worker");
        
        var queueName = "kitchen-orderSubmitted-worker";

        var subscription = this._eventSubscriber.CreateEventConsumer(queueName, "order.orderSubmitted.v1");
        
        subscription.Consumer.Received += async (model, ea) =>
        {
            this._logger.LogInformation("Received event, processing");
            
            var evtDataResponse = await _eventSubscriber.ParseEventFrom<OrderSubmittedEventV1>(ea.Body.ToArray());

            using var processingActivity = _source.StartActivity("kitchen-process-order-submitted-event",
                ActivityKind.Server, evtDataResponse.TraceParent);
            processingActivity.AddTag("queue.time", evtDataResponse.QueueTime);

            processingActivity.SetTag("orderIdentifier", evtDataResponse.EventData.OrderIdentifier);
            
            this._logger.LogInformation($"Event is for order {evtDataResponse.EventData.OrderIdentifier}");

            await this._eventHandler.Handle(evtDataResponse.EventData);
        };
        
        while (!stoppingToken.IsCancellationRequested)
        {
            subscription.Channel.BasicConsume(
                queueName,
                autoAck: true,
                consumer: subscription.Consumer);

            await Task.Delay(1000, stoppingToken);
        }
    }
}