using System.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using PlantBasedPizza.Events;
using PlantBasedPizza.Orders.Worker.Handlers;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;
using RabbitMQ.Client;

namespace PlantBasedPizza.Orders.Worker;

public class OrderPrepCompleteEventWorker : BackgroundService
{
    private readonly RabbitMqEventSubscriber _eventSubscriber;
    private readonly ActivitySource _source;
    private readonly OrderPrepCompleteEventHandler _eventHandler;

    public OrderPrepCompleteEventWorker(RabbitMqEventSubscriber eventSubscriber, ActivitySource source, OrderPrepCompleteEventHandler eventHandler)
    {
        _eventSubscriber = eventSubscriber;
        _source = source;
        _eventHandler = eventHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueName = "orders-orderPrepComplete-worker";

        var subscription = this._eventSubscriber.CreateEventConsumer(queueName, "kitchen.orderPrepComplete.v1");
        
        subscription.Consumer.Received += async (model, ea) =>
        {
            var evtDataResponse = await _eventSubscriber.ParseEventFrom<OrderPrepCompleteEventV1>(ea.Body.ToArray());

            using var processingActivity = _source.StartActivity("processing-order-prep-complete-event",
                ActivityKind.Server, evtDataResponse.TraceParent);
            processingActivity.AddTag("queue.time", evtDataResponse.QueueTime);

            processingActivity.SetTag("orderIdentifier", evtDataResponse.EventData.OrderIdentifier);

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