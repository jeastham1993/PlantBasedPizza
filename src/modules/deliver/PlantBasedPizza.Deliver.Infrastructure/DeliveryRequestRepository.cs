using System.Diagnostics;
using MongoDB.Driver;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Deliver.Infrastructure
{
    public class DeliveryRequestRepository : IDeliveryRequestRepository
    {
        private readonly IMongoCollection<DeliveryRequest> _collection;

        public DeliveryRequestRepository(MongoClient client)
        {
            var database = client.GetDatabase("PlantBasedPizza");
            this._collection = database.GetCollection<DeliveryRequest>("DeliveryRequests");
        }
        
        public async Task AddNewDeliveryRequest(DeliveryRequest request)
        {
            await this._collection.InsertOneAsync(request).ConfigureAwait(false);
        }

        public async Task UpdateDeliveryRequest(DeliveryRequest request)
        {
            var queryBuilder = Builders<DeliveryRequest>.Filter.Eq(ord => ord.OrderIdentifier, request.OrderIdentifier);

            var replaceResult = await this._collection.ReplaceOneAsync(queryBuilder, request);
            
            replaceResult.AddToTelemetry();
        }

        public async Task<DeliveryRequest?> GetDeliveryStatusForOrder(string orderIdentifier)
        {
            var queryBuilder = Builders<DeliveryRequest>.Filter.Eq(p => p.OrderIdentifier, orderIdentifier);

            var request = await this._collection.Find(queryBuilder).FirstOrDefaultAsync().ConfigureAwait(false);

            return request;
        }

        public async Task<List<DeliveryRequest>> GetAwaitingDriver()
        {
            var requests = await this._collection.Find(p => p.DriverCollectedOn == null).ToListAsync();

            Activity.Current?.AddTag("mongo.findCount", requests.Count);

            return requests;
        }

        public async Task<List<DeliveryRequest>> GetOrdersWithDriver(string driverName)
        {
            var requests = await this._collection.Find(p => p.DeliveredOn == null && p.DriverCollectedOn != null && p.Driver == driverName).ToListAsync();
            
            Activity.Current?.AddTag("mongo.findCount", requests.Count);

            return requests;
        }
    }
}