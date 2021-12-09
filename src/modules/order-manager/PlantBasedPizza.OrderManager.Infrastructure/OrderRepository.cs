﻿using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using PlantBasedPizza.OrderManager.Core.Entites;
using PlantBasedPizza.OrderManager.Infrastructure;

public class OrderRepository : IOrderRepository
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<Order> _orders;

    public OrderRepository(MongoClient client)
    {
        this._database = client.GetDatabase("PlantBasedPizza");
        this._orders = this._database.GetCollection<Order>("orders");
    }

    public async Task Add(Order order)
    {
        await this._orders.InsertOneAsync(order).ConfigureAwait(false);
    }

    public async Task<Order> Retrieve(string orderIdentifier)
    {
        var queryBuilder = Builders<Order>.Filter.Eq(p => p.OrderIdentifier, orderIdentifier);

        var order = await this._orders.Find(queryBuilder).FirstOrDefaultAsync().ConfigureAwait(false);

        return order;
    }

    public async Task<List<Order>> GetAwaitingCollection()
    {
        var queryBuilder = Builders<Order>.Filter.Eq(p => p.OrderType, OrderType.PICKUP);

        var order = await this._orders.Find(p => p.OrderType == OrderType.PICKUP && p.AwaitingCollection == true).ToListAsync();

        return order;
    }

    public async Task Update(Order order)
    {
        var queryBuilder = Builders<Order>.Filter.Eq(ord => ord.OrderIdentifier, order.OrderIdentifier);

        await this._orders.ReplaceOneAsync(queryBuilder, order);
    }
}