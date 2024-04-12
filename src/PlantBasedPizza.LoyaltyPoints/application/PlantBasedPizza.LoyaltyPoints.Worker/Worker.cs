using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Mime;
using System.Text;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlantBasedPizza.Events;
using PlantBasedPizza.LoyaltyPoints.Shared.Core;
using RabbitMQ.Client;

namespace PlantBasedPizza.LoyaltyPoints.Worker;

public class Worker : BackgroundService
{
    private readonly AddLoyaltyPointsCommandHandler _handler;
    private readonly RabbitMqEventSubscriber _subscriber;
    private readonly ActivitySource _source;

    public Worker(AddLoyaltyPointsCommandHandler handler, RabbitMqEventSubscriber subscriber, ILogger<Worker> logger, ActivitySource source)
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
            var evtDataResponse = await _subscriber.ParseEventFrom<OrderCompletedEvent>(ea.Body.ToArray());

            using var processingActivity = _source.StartActivity("processing-order-completed-event",
                ActivityKind.Server, evtDataResponse.TraceParent);
            processingActivity.AddTag("queue.time", evtDataResponse.QueueTime);

            await this._handler.Handle(new AddLoyaltyPointsCommand()
            {
                CustomerIdentifier = evtDataResponse.EventData.CustomerIdentifier,
                OrderValue = evtDataResponse.EventData.OrderValue,
                OrderIdentifier = evtDataResponse.EventData.OrderIdentifier
            });
        };
        
        while (!stoppingToken.IsCancellationRequested)
        {
            eventConsumer.Channel.BasicConsume(
                queueName,
                autoAck: true,
                consumer: eventConsumer.Consumer);

            await Task.Delay(1000, stoppingToken);
        }
    }
}