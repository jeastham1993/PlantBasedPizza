using System.Diagnostics;
using MongoDB.Driver;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Kitchen.Infrastructure;

public class KitchenRequestRepository : IKitchenRequestRepository
{
    private readonly IMongoCollection<KitchenRequest> _kitchenRequests;

    public KitchenRequestRepository(MongoClient client)
    {
        var database = client.GetDatabase("PlantBasedPizza");
        this._kitchenRequests = database.GetCollection<KitchenRequest>("kitchen");
    }

    public async Task AddNew(KitchenRequest kitchenRequest)
    {
        using var dataAccessActivity = Activity.Current?.Source.StartActivity("DataAccess");
        
        await this._kitchenRequests.InsertOneAsync(kitchenRequest).ConfigureAwait(false);
    }

    public async Task Update(KitchenRequest kitchenRequest)
    {
        using var dataAccessActivity = Activity.Current?.Source.StartActivity("DataAccess");
        
        var queryBuilder = Builders<KitchenRequest>.Filter.Eq(req => req.OrderIdentifier, kitchenRequest.OrderIdentifier);

        var updateResult = await this._kitchenRequests.ReplaceOneAsync(queryBuilder, kitchenRequest);
        
        updateResult.AddToTelemetry();
    }

    public async Task<KitchenRequest> Retrieve(string orderIdentifier)
    {
        using var dataAccessActivity = Activity.Current?.Source.StartActivity("DataAccess");
        
        var queryBuilder = Builders<KitchenRequest>.Filter.Eq(p => p.OrderIdentifier, orderIdentifier);

        var kitchenRequest = await this._kitchenRequests.Find(queryBuilder).FirstOrDefaultAsync().ConfigureAwait(false);

        return kitchenRequest;
    }

    public async Task<IEnumerable<KitchenRequest>> GetNew()
    {
        using var dataAccessActivity = Activity.Current?.Source.StartActivity("DataAccess");
        
        var queryBuilder = Builders<KitchenRequest>.Filter.Eq(p => p.OrderState, OrderState.NEW);

        var kitchenRequests = await this._kitchenRequests.FindAsync(queryBuilder).ConfigureAwait(false);

        return await kitchenRequests.ToListAsync();
    }

    public async Task<IEnumerable<KitchenRequest>> GetPrep()
    {
        using var dataAccessActivity = Activity.Current?.Source.StartActivity("DataAccess");
        
        var queryBuilder = Builders<KitchenRequest>.Filter.Eq(p => p.OrderState, OrderState.PREPARING);

        var kitchenRequests = await this._kitchenRequests.FindAsync(queryBuilder).ConfigureAwait(false);

        return await kitchenRequests.ToListAsync();
    }

    public async Task<IEnumerable<KitchenRequest>> GetBaking()
    {
        using var dataAccessActivity = Activity.Current?.Source.StartActivity("DataAccess");
        
        var queryBuilder = Builders<KitchenRequest>.Filter.Eq(p => p.OrderState, OrderState.BAKING);

        var kitchenRequests = await this._kitchenRequests.FindAsync(queryBuilder).ConfigureAwait(false);

        return await kitchenRequests.ToListAsync();
    }

    public async Task<IEnumerable<KitchenRequest>> GetAwaitingQualityCheck()
    {
        using var dataAccessActivity = Activity.Current?.Source.StartActivity("DataAccess");
        
        var queryBuilder = Builders<KitchenRequest>.Filter.Eq(p => p.OrderState, OrderState.QUALITYCHECK);

        var kitchenRequests = await this._kitchenRequests.FindAsync(queryBuilder).ConfigureAwait(false);

        return await kitchenRequests.ToListAsync();
    }
}