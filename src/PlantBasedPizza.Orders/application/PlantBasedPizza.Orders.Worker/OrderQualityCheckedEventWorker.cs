using System.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using PlantBasedPizza.Events;
using PlantBasedPizza.Orders.Worker.Handlers;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;
using RabbitMQ.Client;

namespace PlantBasedPizza.Orders.Worker;

public class OrderQualityCheckedEventWorker : BackgroundService
{
    private readonly RabbitMqEventSubscriber _eventSubscriber;
    private readonly ActivitySource _source;
    private readonly OrderQualityCheckedEventHandler _eventHandler;

    public OrderQualityCheckedEventWorker(RabbitMqEventSubscriber eventSubscriber, ActivitySource source, OrderQualityCheckedEventHandler eventHandler)
    {
        _eventSubscriber = eventSubscriber;
        _source = source;
        _eventHandler = eventHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueName = "orders-qualityChecked-worker";

        var subscription = this._eventSubscriber.CreateEventConsumer(queueName, "kitchen.qualityChecked.v1");
        
        subscription.Consumer.Received += async (model, ea) =>
        {
            var evtDataResponse = await _eventSubscriber.ParseEventFrom<OrderQualityCheckedEventV1>(ea.Body.ToArray());

            using var processingActivity = _source.StartActivity("processing-order-quality-checked-event",
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