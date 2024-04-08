using System.Diagnostics;
using MongoDB.Driver;
using PlantBasedPizza.Events;
using PlantBasedPizza.LoyaltyPoints.Shared.Core;

namespace PlantBasedPizza.LoyaltyPoints.Shared.Adapters;

public class CustomerLoyaltyPointRepository : ICustomerLoyaltyPointsRepository
{
    private readonly IMongoCollection<CustomerLoyaltyPoints> _loyaltyPoints;
    private readonly IEventPublisher _eventPublisher;

    public CustomerLoyaltyPointRepository(MongoClient client, IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
        var database = client.GetDatabase("LoyaltyPoints");
        this._loyaltyPoints = database.GetCollection<CustomerLoyaltyPoints>("loyalty");
    }
    
    public async Task<CustomerLoyaltyPoints?> GetCurrentPointsFor(string customerIdentifier)
    {
        var queryBuilder = Builders<CustomerLoyaltyPoints>.Filter.Eq(p => p.CustomerId, customerIdentifier);

        var currentPoints = await this._loyaltyPoints.Find(queryBuilder).FirstOrDefaultAsync();
        
        if (currentPoints == null)
        {
            Activity.Current?.AddTag("loyalty.notFoundForCustomer", true);
        }

        return currentPoints;
    }

    public async Task UpdatePoints(Core.CustomerLoyaltyPoints points)
    {
        var queryBuilder = Builders<CustomerLoyaltyPoints>.Filter.Eq(p => p.CustomerId, points.CustomerId);

        var updateDefinition = Builders<CustomerLoyaltyPoints>.Update
            .Set(loyaltyPoint => loyaltyPoint.TotalPoints, points.TotalPoints)
            .Set(loyaltyPoint => loyaltyPoint.History, points.History);

        await this._loyaltyPoints.UpdateOneAsync(queryBuilder, updateDefinition, new UpdateOptions() { IsUpsert = true });

        await this._eventPublisher.Publish(new CustomerLoyaltyPointsUpdated()
        {
            CustomerIdentifier = points.CustomerId,
            TotalLoyaltyPoints = points.TotalPoints
        });
    }
}