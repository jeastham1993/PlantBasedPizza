﻿using System.Diagnostics;
using MongoDB.Driver;
using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class OrderRepository : IOrderRepository
{
    private readonly IMongoCollection<Order> _orders;

    public OrderRepository(MongoClient client)
    {
        var database = client.GetDatabase("PlantBasedPizza");
        _orders = database.GetCollection<Order>("orders");
    }

    public async Task Add(Order order)
    {
        await _orders.InsertOneAsync(order).ConfigureAwait(false);
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

    public async Task<bool> Exists(string orderIdentifier)
    {
        var queryBuilder = Builders<Order>.Filter.Eq(p => p.OrderIdentifier, orderIdentifier);

        var order = await _orders.Find(queryBuilder).FirstOrDefaultAsync().ConfigureAwait(false);

        return order != null;
    }

    public async Task<List<Order>> GetAwaitingCollection()
    {
        var order = await _orders.Find(p => p.OrderType == OrderType.Pickup && p.AwaitingCollection).ToListAsync();

        return order;
    }

    public async Task Update(Order order)
    {
        var queryBuilder = Builders<Order>.Filter.Eq(ord => ord.OrderIdentifier, order.OrderIdentifier);

        await _orders.ReplaceOneAsync(queryBuilder, order);
    }
}