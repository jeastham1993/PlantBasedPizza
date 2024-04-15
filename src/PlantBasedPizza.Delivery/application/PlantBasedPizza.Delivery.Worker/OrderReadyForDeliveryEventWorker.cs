using System.Diagnostics;
using PlantBasedPizza.Deliver.Core.Handlers;
using PlantBasedPizza.Deliver.Core.IntegrationEvents;
using PlantBasedPizza.Events;
using RabbitMQ.Client;

namespace PlantBasedPizza.Delivery.Worker;

public class OrderReadyForDeliveryEventWorker(
    RabbitMqEventSubscriber eventSubscriber,
    ActivitySource source,
    OrderReadyForDeliveryEventHandler eventHandler)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueName = "delivery-readyForDelivery-worker";

        var subscription = eventSubscriber.CreateEventConsumer(queueName, "order.readyForDelivery.v1");
        
        subscription.Consumer.Received += async (model, ea) =>
        {
            var evtDataResponse = await eventSubscriber.ParseEventFrom<OrderReadyForDeliveryEventV1>(ea.Body.ToArray());

            using var processingActivity = source.StartActivity("processing-order-quality-checked-event",
                ActivityKind.Server, evtDataResponse.TraceParent);
            processingActivity.AddTag("queue.time", evtDataResponse.QueueTime);

            processingActivity.SetTag("orderIdentifier", evtDataResponse.EventData.OrderIdentifier);

            await eventHandler.Handle(evtDataResponse.EventData);
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