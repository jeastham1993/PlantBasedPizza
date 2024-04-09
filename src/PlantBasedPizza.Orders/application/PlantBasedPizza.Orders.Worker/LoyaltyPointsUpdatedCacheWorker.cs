using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Caching.Distributed;
using PlantBasedPizza.Events;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace PlantBasedPizza.Orders.Worker;

public class LoyaltyPointsUpdatedCacheWorker : BackgroundService
{
    private readonly RabbitMqEventSubscriber _eventSubscriber;
    private readonly ActivitySource _source;
    private readonly IDistributedCache _distributedCache;

    public LoyaltyPointsUpdatedCacheWorker(RabbitMqEventSubscriber eventSubscriber, ActivitySource source, IDistributedCache distributedCache)
    {
        _eventSubscriber = eventSubscriber;
        _source = source;
        _distributedCache = distributedCache;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueName = "orders-loyaltyPointsUpdated-worker";

        var subscription =
            await this._eventSubscriber.CreateEventConsumer(queueName, "loyalty.customerLoyaltyPointsUpdated.v1");
        
        subscription.Consumer.Received += async (model, ea) =>
        {
            var evtDataResponse = await _eventSubscriber.ParseEventFrom<CustomerLoyaltyPointsUpdatedEvent>(ea.Body.ToArray());

            using var processingActivity = _source.StartActivity("processing-order-completed-event",
                ActivityKind.Server, evtDataResponse.TraceParent);
            processingActivity.AddTag("queue.time", evtDataResponse.QueueTime);

            processingActivity.SetTag("customerIdentifier", evtDataResponse.EventData.CustomerIdentifier);
            processingActivity.SetTag("totalPoints", evtDataResponse.EventData.TotalLoyaltyPoints);

            await this._distributedCache.SetStringAsync(evtDataResponse.EventData.CustomerIdentifier.ToUpper(),
                evtDataResponse.EventData.TotalLoyaltyPoints.ToString("n0"), token: stoppingToken);
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