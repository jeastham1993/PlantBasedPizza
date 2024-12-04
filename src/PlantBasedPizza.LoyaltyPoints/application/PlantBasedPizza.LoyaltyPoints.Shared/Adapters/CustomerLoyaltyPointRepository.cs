using System.Diagnostics;
using Dapr.Client;
using MongoDB.Driver;
using PlantBasedPizza.Events;
using PlantBasedPizza.LoyaltyPoints.Shared.Core;

namespace PlantBasedPizza.LoyaltyPoints.Shared.Adapters;

public class CustomerLoyaltyPointRepository : ICustomerLoyaltyPointsRepository
{
    private readonly IMongoCollection<CustomerLoyaltyPoints> _loyaltyPoints;
    private readonly DaprClient _daprClient;
    private const string SOURCE = "loyalty";
    private const string PUB_SUB_NAME = "public";
    private const string DATE_FORMAT = "yyyy-MM-ddTHH:mm:ssZ";

    public CustomerLoyaltyPointRepository(MongoClient client, DaprClient daprClient)
    {
        _daprClient = daprClient;
        var database = client.GetDatabase("LoyaltyPoints");
        _loyaltyPoints = database.GetCollection<CustomerLoyaltyPoints>("loyalty");
    }
    
    public async Task<CustomerLoyaltyPoints?> GetCurrentPointsFor(string customerIdentifier)
    {
        var queryBuilder = Builders<CustomerLoyaltyPoints>.Filter.Eq(p => p.CustomerId, customerIdentifier);

        var currentPoints = await _loyaltyPoints.Find(queryBuilder).FirstOrDefaultAsync();
        
        if (currentPoints == null)
        {
            Activity.Current?.AddTag("loyalty.notFoundForCustomer", true);
        }

        return currentPoints;
    }

    public async Task UpdatePoints(CustomerLoyaltyPoints points)
    {
        var queryBuilder = Builders<CustomerLoyaltyPoints>.Filter.Eq(p => p.CustomerId, points.CustomerId);

        var updateDefinition = Builders<CustomerLoyaltyPoints>.Update
            .Set(loyaltyPoint => loyaltyPoint.TotalPoints, points.TotalPoints)
            .Set(loyaltyPoint => loyaltyPoint.History, points.History);

        await _loyaltyPoints.UpdateOneAsync(queryBuilder, updateDefinition, new UpdateOptions() { IsUpsert = true });

        var evt = new CustomerLoyaltyPointsUpdated()
        {
            CustomerIdentifier = points.CustomerId,
            TotalLoyaltyPoints = points.TotalPoints
        };
        
        var eventType = $"{evt.EventName}.{evt.EventVersion}";
        var eventId = Guid.NewGuid().ToString();
        
        evt.AddToTelemetry(eventId);
        
        var eventMetadata = new Dictionary<string, string>(3)
        {
            { EventConstants.EVENT_SOURCE_HEADER_KEY, SOURCE },
            { EventConstants.EVENT_TYPE_HEADER_KEY, eventType},
            { EventConstants.EVENT_ID_HEADER_KEY, eventId },
            { EventConstants.EVENT_TIME_HEADER_KEY, DateTime.UtcNow.ToString(DATE_FORMAT) },
        };
        
        await _daprClient.PublishEventAsync(PUB_SUB_NAME, eventType, evt, eventMetadata);
    }
}