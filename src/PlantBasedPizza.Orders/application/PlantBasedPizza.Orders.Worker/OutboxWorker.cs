using System.Text.Json;
using MongoDB.Driver;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.OrderSubmitted;
using PlantBasedPizza.OrderManager.Core.PublicEvents;
using PlantBasedPizza.OrderManager.Infrastructure;

namespace PlantBasedPizza.Orders.Worker;

public class OutboxWorker : BackgroundService
{
    private readonly IMongoCollection<OutboxItem> _outboxItems;
    private readonly ILogger<OutboxWorker> _logger;
    private readonly IOrderEventPublisher _eventPublisher;
    
    public OutboxWorker(MongoClient client, ILogger<OutboxWorker> logger, IOrderEventPublisher eventPublisher)
    {
        _logger = logger;
        _eventPublisher = eventPublisher;
        var database = client.GetDatabase("PlantBasedPizza");
        _outboxItems = database.GetCollection<OutboxItem>("orders_outboxitems");
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var outboxItems = await _outboxItems.Find(p => p.Processed == false).ToListAsync();
            
            _logger.LogInformation("Outbox Items: {OutboxItems}", outboxItems.Count);

            foreach (var outboxItem in outboxItems)
            {
                try
                {
                    _logger.LogInformation("Processing outbox item: Type: {OutboxItemType}. Data: {OutboxItemData}", outboxItem.EventType, outboxItem.EventData);

                    switch (outboxItem.EventType)
                    {
                        case nameof(OrderCompletedIntegrationEventV1):
                            var orderCompletedIntegrationEvt = JsonSerializer.Deserialize<OrderCompletedIntegrationEventV1>(outboxItem.EventData);
                            await _eventPublisher.PublishOrderCompletedEventV1(orderCompletedIntegrationEvt);
                            outboxItem.Processed = true;
                            break;
                        case nameof(OrderReadyForDeliveryEventV1):
                            var orderReadyForDeliveryEvt = JsonSerializer.Deserialize<OrderReadyForDeliveryEventV1>(outboxItem.EventData);
                            await _eventPublisher.PublishOrderReadyForDeliveryEventV1(orderReadyForDeliveryEvt);
                            outboxItem.Processed = true;
                            break;
                        case nameof(OrderConfirmedEventV1):
                            var orderSubmittedEvt = JsonSerializer.Deserialize<OrderConfirmedEventV1>(outboxItem.EventData);
                            await _eventPublisher.PublishOrderConfirmedEventV1(orderSubmittedEvt);
                            outboxItem.Processed = true;
                            break;
                        case nameof(OrderCreatedEventV1):
                            var orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEventV1>(outboxItem.EventData);
                            await _eventPublisher.PublishOrderCreatedEventV1(orderCreatedEvent);
                            outboxItem.Processed = true;
                            break;
                        case nameof(OrderSubmittedEventV1):
                            var submittedEvent = JsonSerializer.Deserialize<OrderSubmittedEventV1>(outboxItem.EventData);
                            await _eventPublisher.PublishOrderSubmittedEventV1(submittedEvent);
                            outboxItem.Processed = true;
                            break;
                        default:
                            _logger.LogWarning("Unknown event type: {EventType}", outboxItem.EventType);
                            outboxItem.Processed = true;
                            break;
                    }
                
                    var queryBuilder = Builders<OutboxItem>.Filter.Eq(item => item.ItemId, outboxItem.ItemId);
            
                    await _outboxItems.ReplaceOneAsync(queryBuilder, outboxItem, cancellationToken: stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An error occured while processing outbox item.");
                }
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}