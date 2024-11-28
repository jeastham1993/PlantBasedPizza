using System.Diagnostics;
using MongoDB.Driver;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Deliver.Infrastructure
{
    public class DeliveryRequestRepository : IDeliveryRequestRepository
    {
        private readonly IMongoCollection<DeliveryRequest> _collection;
        private readonly IMongoCollection<OutboxItem> _outboxItems;

        public DeliveryRequestRepository(MongoClient client)
        {
            var database = client.GetDatabase("PlantBasedPizza");
            _collection = database.GetCollection<DeliveryRequest>("DeliveryRequests");
            _outboxItems = database.GetCollection<OutboxItem>("DeliveryRequests_outboxitems");
        }
        
        public async Task AddNewDeliveryRequest(DeliveryRequest request, List<IntegrationEvent> events = null)
        {
            await _collection.InsertOneAsync(request).ConfigureAwait(false);
            
            foreach (var evt in (events ?? new()))
            {
                await _outboxItems.InsertOneAsync(new OutboxItem()
                {
                    EventData = evt.AsString(),
                    EventType = evt.GetType().Name,
                    Processed = false
                });
            }
        }

        public async Task UpdateDeliveryRequest(DeliveryRequest request, List<IntegrationEvent> events = null)
        {
            var queryBuilder = Builders<DeliveryRequest>.Filter.Eq(ord => ord.OrderIdentifier, request.OrderIdentifier);

            var replaceResult = await _collection.ReplaceOneAsync(queryBuilder, request);
            
            foreach (var evt in (events ?? new()))
            {
                await _outboxItems.InsertOneAsync(new OutboxItem()
                {
                    EventData = evt.AsString(),
                    EventType = evt.GetType().Name,
                    Processed = false
                });
            }
            
            replaceResult.AddToTelemetry();
        }

        public async Task<DeliveryRequest?> GetDeliveryStatusForOrder(string orderIdentifier)
        {
            var queryBuilder = Builders<DeliveryRequest>.Filter.Eq(p => p.OrderIdentifier, orderIdentifier);

            var request = await _collection.Find(queryBuilder).FirstOrDefaultAsync().ConfigureAwait(false);

            return request;
        }

        public async Task<List<DeliveryRequest>> GetAwaitingDriver()
        {
            var requests = await _collection.Find(p => p.DriverCollectedOn == null).ToListAsync();

            Activity.Current?.AddTag("mongo.findCount", requests.Count);

            return requests;
        }

        public async Task<List<DeliveryRequest>> GetOrdersWithDriver(string driverName)
        {
            var requests = await _collection.Find(p => p.DeliveredOn == null && p.DriverCollectedOn != null && p.Driver == driverName).ToListAsync();
            
            Activity.Current?.AddTag("mongo.findCount", requests.Count);

            return requests;
        }
    }
}