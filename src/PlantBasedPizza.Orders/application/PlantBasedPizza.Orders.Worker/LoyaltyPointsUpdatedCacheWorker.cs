using System.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using PlantBasedPizza.Events;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;

namespace PlantBasedPizza.Orders.Worker;

public class LoyaltyPointsUpdatedCacheWorker : BackgroundService
{
    private readonly SqsEventSubscriber _eventSubscriber;
    private readonly ActivitySource _source;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<LoyaltyPointsUpdatedCacheWorker> _logger;
    private readonly QueueConfiguration _queueConfiguration;

    public LoyaltyPointsUpdatedCacheWorker(SqsEventSubscriber eventSubscriber, ActivitySource source,
        IDistributedCache distributedCache, ILogger<LoyaltyPointsUpdatedCacheWorker> logger, IOptions<QueueConfiguration> queueConfiguration)
    {
        _eventSubscriber = eventSubscriber;
        _source = source;
        _distributedCache = distributedCache;
        _logger = logger;
        _queueConfiguration = queueConfiguration.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueUrl = await this._eventSubscriber.GetQueueUrl(_queueConfiguration.LoyaltyPointsUpdatedQueue);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = await _eventSubscriber.GetMessages<CustomerLoyaltyPointsUpdatedEvent>(queueUrl);

            foreach (var message in messages)
            {
                using var processingActivity = _source.StartActivity("processing-order-completed-event",
                    ActivityKind.Server, message.TraceParent);
                processingActivity.AddTag("queue.time", message.QueueTime);
                processingActivity.AddTag("customerIdentifier", message.EventData.CustomerIdentifier);
                processingActivity.AddTag("totalPoints", message.EventData.TotalLoyaltyPoints);

                await _distributedCache.SetStringAsync(message.EventData.CustomerIdentifier.ToUpper(),
                    message.EventData.TotalLoyaltyPoints.ToString("n0"), stoppingToken);

                this._logger.LogInformation("Cached");
                
                await _eventSubscriber.Ack(queueUrl, message);
            }
            
            await Task.Delay(1000, stoppingToken);
        }
    }
}