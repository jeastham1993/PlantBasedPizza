using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using PlantBasedPizza.Deliver.Core.Handlers;
using PlantBasedPizza.Deliver.Core.IntegrationEvents;
using PlantBasedPizza.Events;
using RabbitMQ.Client;

namespace PlantBasedPizza.Deliver.Infrastructure;

public class OrderReadyForDeliveryEventWorker : BackgroundService
{
    private readonly RabbitMqEventSubscriber _eventSubscriber;
    private readonly ActivitySource _source;
    private readonly OrderReadyForDeliveryEventHandler _eventHandler;

    public OrderReadyForDeliveryEventWorker(RabbitMqEventSubscriber eventSubscriber, ActivitySource source, OrderReadyForDeliveryEventHandler eventHandler)
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
            var evtDataResponse = await _eventSubscriber.ParseEventFrom<OrderReadyForDeliveryEventV1>(ea.Body.ToArray());

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