using System.Diagnostics;
using PlantBasedPizza.Events;
using PlantBasedPizza.LoyaltyPoints.Shared.Core;
using RabbitMQ.Client;

namespace PlantBasedPizza.LoyaltyPoints.Worker;

public class AddLoyaltyPointsWorker : BackgroundService
{
    private readonly AddLoyaltyPointsCommandHandler _handler;
    private readonly RabbitMqEventSubscriber _subscriber;
    private readonly ActivitySource _source;

    public AddLoyaltyPointsWorker(AddLoyaltyPointsCommandHandler handler, RabbitMqEventSubscriber subscriber, ILogger<AddLoyaltyPointsWorker> logger,
        ActivitySource source)
    {
        _handler = handler;
        _subscriber = subscriber;
        _source = source;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueName = "loyaltypoints-ordercompleted-worker";

        var eventConsumer = _subscriber.CreateEventConsumer(queueName, "order.orderCompleted.v1");

        eventConsumer.Consumer.Received += async (model, ea) =>
        {
            try
            {
                var evtDataResponse = await _subscriber.ParseEventFrom<OrderCompletedEvent>(ea.Body.ToArray());

                using var processingActivity = _source.StartActivity("processing-order-completed-event",
                    ActivityKind.Server, evtDataResponse.TraceParent);
                processingActivity.AddTag("queue.time", evtDataResponse.QueueTime);

                await _handler.Handle(new AddLoyaltyPointsCommand
                {
                    CustomerIdentifier = evtDataResponse.EventData.CustomerIdentifier,
                    OrderValue = evtDataResponse.EventData.OrderValue,
                    OrderIdentifier = evtDataResponse.EventData.OrderIdentifier
                });
                
                eventConsumer.Channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception e)
            {
                eventConsumer.Channel.BasicReject(ea.DeliveryTag, true);
            }
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            eventConsumer.Channel.BasicConsume(
                queueName,
                false,
                eventConsumer.Consumer);

            await Task.Delay(1000, stoppingToken);
        }
    }
}