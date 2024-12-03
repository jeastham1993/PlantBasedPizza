using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;
using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class OrderRepository : IOrderRepository
{
    private readonly IMongoCollection<Order> _orders;
    private readonly IMongoCollection<OutboxItem> _outboxItems;
    private readonly IDistributedCache _cache;

    public OrderRepository(MongoClient client, IDistributedCache cache)
    {
        _cache = cache;
        var database = client.GetDatabase("PlantBasedPizza");
        _orders = database.GetCollection<Order>("orders");
        _outboxItems = database.GetCollection<OutboxItem>("orders_outboxitems");
    }

    public async Task Add(Order order)
    {
        await _orders.InsertOneAsync(order).ConfigureAwait(false);
        
        await _cache.SetStringAsync(order.OrderIdentifier, JsonSerializer.Serialize(new OrderDto(order)));

        foreach (var evt in order.Events)
        {
            await _outboxItems.InsertOneAsync(new OutboxItem()
            {
                EventData = evt.AsString(),
                EventType = evt.GetType().Name,
                Processed = false
            });
        }
    }

    public async Task<Order> Retrieve(string orderIdentifier)
    {
        var queryBuilder = Builders<Order>
            .Filter
            .Eq(p => p.OrderIdentifier, orderIdentifier);

        var order = await _orders.Find(queryBuilder).FirstOrDefaultAsync().ConfigureAwait(false);

        if (order == null)
        {
            Activity.Current?.AddTag("order.notFound", true);
            throw new OrderNotFoundException(orderIdentifier);
        }

        return order;
    }

    public async Task<List<Order>> GetAwaitingCollection()
    {
        var order = await _orders.Find(p => p.OrderType == OrderType.Pickup && p.AwaitingCollection).ToListAsync();

        return order;
    }

    public async Task<List<Order>> ForCustomer(string accountId)
    {
        var order = await _orders.Find(p => p.CustomerIdentifier.ToLower() == accountId.ToLower()).ToListAsync();

        return order;
    }

    public async Task Update(Order order)
    {
        // TODO: This should be wrapped in a transaction
        var queryBuilder = Builders<Order>.Filter.Eq(ord => ord.OrderIdentifier, order.OrderIdentifier);
            
        await _orders.ReplaceOneAsync(queryBuilder, order);
        
        await _cache.SetStringAsync(order.OrderIdentifier, JsonSerializer.Serialize(new OrderDto(order)));

        foreach (var evt in order.Events)
        {
            await _outboxItems.InsertOneAsync(new OutboxItem()
            {
                EventData = evt.AsString(),
                EventType = evt.GetType().Name,
                Processed = false
            });
        }
    }
}