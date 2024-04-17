using System.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using PlantBasedPizza.Events;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;
using RabbitMQ.Client;

namespace PlantBasedPizza.Orders.Worker;

public class LoyaltyPointsUpdatedCacheWorker : BackgroundService
{
    private readonly RabbitMqEventSubscriber _eventSubscriber;
    private readonly ActivitySource _source;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<LoyaltyPointsUpdatedCacheWorker> _logger;

    public LoyaltyPointsUpdatedCacheWorker(RabbitMqEventSubscriber eventSubscriber, ActivitySource source,
        IDistributedCache distributedCache, ILogger<LoyaltyPointsUpdatedCacheWorker> logger)
    {
        _eventSubscriber = eventSubscriber;
        _source = source;
        _distributedCache = distributedCache;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueName = "orders-loyaltyPointsUpdated-worker";

        var subscription = _eventSubscriber.CreateEventConsumer(queueName, "loyalty.customerLoyaltyPointsUpdated.v1");

        subscription.Consumer.Received += async (model, ea) =>
        {
            try
            {
                this._logger.LogInformation("Processing message {messageId}", ea.DeliveryTag);
                
                var evtDataResponse =
                    await _eventSubscriber.ParseEventFrom<CustomerLoyaltyPointsUpdatedEvent>(ea.Body.ToArray());

                using var processingActivity = _source.StartActivity("processing-order-completed-event",
                    ActivityKind.Server, evtDataResponse.TraceParent);
                processingActivity.AddTag("queue.time", evtDataResponse.QueueTime);
                processingActivity.AddTag("message.id", ea.DeliveryTag);
                processingActivity.AddTag("customerIdentifier", evtDataResponse.EventData.CustomerIdentifier);
                processingActivity.AddTag("totalPoints", evtDataResponse.EventData.TotalLoyaltyPoints);

                await _distributedCache.SetStringAsync(evtDataResponse.EventData.CustomerIdentifier.ToUpper(),
                    evtDataResponse.EventData.TotalLoyaltyPoints.ToString("n0"), stoppingToken);

                this._logger.LogInformation("Cached");

                subscription.Channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception e)
            {
                this._logger.LogError(e, "Failure processing message");
                
                subscription.Channel.BasicReject(ea.DeliveryTag, true);
            }
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            subscription.Channel.BasicConsume(
                queueName,
                false,
                subscription.Consumer);

            await Task.Delay(1000, stoppingToken);
        }
    }
}