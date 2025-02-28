using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entities;
 
namespace PlantBasedPizza.OrderManager.Infrastructure;

public class OutboxWorker : BackgroundService
{
    private readonly IMongoCollection<OutboxItem> _outboxItems;
    private readonly ILogger<OutboxWorker> _logger;
    private readonly OrderEventPublisher _eventPublisher;
    private readonly ActivitySource _source;

    public OutboxWorker(MongoClient client, ILogger<OutboxWorker> logger, OrderEventPublisher eventPublisher)
    {
        _logger = logger;
        _eventPublisher = eventPublisher;
        _source = new ActivitySource(ApplicationDefaults.ServiceName);
        var database = client.GetDatabase("PlantBasedPizza_Monolith");
        _outboxItems = database.GetCollection<OutboxItem>("orders_outboxitems");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var outboxItems = await _outboxItems.Find(p => !p.Processed && !p.Failed).ToListAsync(stoppingToken);

            foreach (var outboxItem in outboxItems)
            {
                try
                {
                    using var processingActivity = StartFromOutboxItem(outboxItem);
                    processingActivity?.Start();

                    _logger.LogInformation("Processing outbox item: Type: {OutboxItemType}. Data: {OutboxItemData}",
                        outboxItem.EventType, outboxItem.EventData);

                    switch (outboxItem.EventType)
                    {
                        case nameof(OrderCompletedEvent):
                            var orderCompletedIntegrationEvt =
                                JsonSerializer.Deserialize<OrderCompletedEvent>(outboxItem.EventData);
                            await _eventPublisher.Publish(orderCompletedIntegrationEvt);
                            outboxItem.Processed = true;
                            break;
                        default:
                            _logger.LogWarning("Unknown event type: {EventType}", outboxItem.EventType);
                            outboxItem.Failed = true;
                            outboxItem.FailureReason = $"Unknown event type: {outboxItem.EventType}";
                            break;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An error occured while processing outbox item.");
                    outboxItem.Failed = true;
                    outboxItem.FailureReason =
                        $"An error occured while processing outbox item.: {e.Message} - {e.StackTrace}";
                }

                var queryBuilder = Builders<OutboxItem>.Filter.Eq(item => item.ItemId, outboxItem.ItemId);
                await _outboxItems.ReplaceOneAsync(queryBuilder, outboxItem, cancellationToken: stoppingToken);
            }

            await Task.Delay(5000, stoppingToken);
        }
    }

    private Activity? StartFromOutboxItem(OutboxItem outboxItem)
    {
        if (!string.IsNullOrEmpty(outboxItem.TraceId))
            try
            {
                var context = ActivityContext.Parse(outboxItem.TraceId, null);
                var messageProcessingActivity = _source.StartActivity("process", ActivityKind.Internal, context);

                return messageProcessingActivity;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failure parsing tracecontext from outbox item");
            }

        return null;
    }
}