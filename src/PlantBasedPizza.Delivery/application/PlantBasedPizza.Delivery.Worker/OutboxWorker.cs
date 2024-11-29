using System.Diagnostics;
using System.Text.Json;
using MongoDB.Driver;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Deliver.Core.PublicEvents;
using PlantBasedPizza.Deliver.Infrastructure;

namespace PlantBasedPizza.Delivery.Worker;

public class OutboxWorker : BackgroundService
{
    private readonly IMongoCollection<OutboxItem> _outboxItems;
    private readonly ILogger<OutboxWorker> _logger;
    private readonly IDeliveryEventPublisher _eventPublisher;
    private readonly ActivitySource _source;
    
    public OutboxWorker(MongoClient client, ILogger<OutboxWorker> logger, IDeliveryEventPublisher eventPublisher)
    {
        _logger = logger;
        _eventPublisher = eventPublisher;
        _source = new ActivitySource(ApplicationDefaults.ServiceName);
        var database = client.GetDatabase("PlantBasedPizza");
        _outboxItems = database.GetCollection<OutboxItem>("DeliveryRequests_outboxitems");
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var outboxItems = await _outboxItems.Find(p => !p.Processed && !p.Failed).ToListAsync(cancellationToken: stoppingToken);

            foreach (var outboxItem in outboxItems)
            {
                try
                {
                    using var processingActivity = StartFromOutboxItem(outboxItem);
                    processingActivity?.Start();
                    
                    _logger.LogInformation("Processing outbox item: Type: {OutboxItemType}. Data: {OutboxItemData}", outboxItem.EventType, outboxItem.EventData);

                    switch (outboxItem.EventType)
                    {
                        case nameof(DriverCollectedOrderEventV1):
                            var orderCompletedIntegrationEvt = JsonSerializer.Deserialize<DriverCollectedOrderEventV1>(outboxItem.EventData);
                            await _eventPublisher.PublishDriverOrderCollectedEventV1(orderCompletedIntegrationEvt);
                            outboxItem.Processed = true;
                            break;
                        case nameof(DriverDeliveredOrderEventV1):
                            var orderReadyForDeliveryEvt = JsonSerializer.Deserialize<DriverDeliveredOrderEventV1>(outboxItem.EventData);
                            await _eventPublisher.PublishDriverDeliveredOrderEventV1(orderReadyForDeliveryEvt);
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
                    outboxItem.FailureReason = $"An error occured while processing outbox item.: {e.Message} - {e.StackTrace}";
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
        {
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
        }

        return null;
    }
}