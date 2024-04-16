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
            try
            {
                var evtDataResponse = await eventSubscriber.ParseEventFrom<OrderReadyForDeliveryEventV1>(ea.Body.ToArray());

                using var processingActivity = source.StartActivity("processing-order-quality-checked-event",
                    ActivityKind.Server, evtDataResponse.TraceParent);
                processingActivity.AddTag("queue.time", evtDataResponse.QueueTime);

                processingActivity.SetTag("orderIdentifier", evtDataResponse.EventData.OrderIdentifier);

                await eventHandler.Handle(evtDataResponse.EventData);
            
                subscription.Channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception e)
            {
                subscription.Channel.BasicReject(ea.DeliveryTag, true);
            }
        };
        
        while (!stoppingToken.IsCancellationRequested)
        {
            subscription.Channel.BasicConsume(
                queueName,
                autoAck: false,
                consumer: subscription.Consumer);

            await Task.Delay(1000, stoppingToken);
        }
    }
}