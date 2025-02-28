﻿using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class OrderRepository : IOrderRepository
{
    private readonly ILogger<OrderRepository> _logger;
    private readonly IMongoCollection<Order> _orders;

    private readonly IMongoCollection<OutboxItem> _outboxItems;

    public OrderRepository(MongoClient client, ILogger<OrderRepository> logger)
    {
        _logger = logger;
        var database = client.GetDatabase("PlantBasedPizza_Monolith");
        _orders = database.GetCollection<Order>("orders");
        _outboxItems = database.GetCollection<OutboxItem>("orders_outboxitems");
    }

    public async Task Add(Order order)
    {
        // TODO: This should be wrapped in a transaction
        await _orders.InsertOneAsync(order).ConfigureAwait(false);
        
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
        var queryBuilder = Builders<Order>.Filter.Eq(p => p.OrderIdentifier, orderIdentifier);

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
        var queryBuilder = Builders<Order>.Filter.Eq(p => p.OrderType, OrderType.Pickup);

        var order = await _orders.Find(p => p.OrderType == OrderType.Pickup && p.AwaitingCollection).ToListAsync();

        return order;
    }

    public async Task Update(Order order)
    {
        var queryBuilder = Builders<Order>.Filter.Eq(ord => ord.OrderIdentifier, order.OrderIdentifier);

        await _orders.ReplaceOneAsync(queryBuilder, order);
        
        foreach (var evt in order.Events)
        {
            this._logger.LogInformation("Writing {evt} to outbox", evt.GetType().Name);
            
            await _outboxItems.InsertOneAsync(new OutboxItem()
            {
                EventData = evt.AsString(),
                EventType = evt.GetType().Name,
                Processed = false
            });
        }
    }
}