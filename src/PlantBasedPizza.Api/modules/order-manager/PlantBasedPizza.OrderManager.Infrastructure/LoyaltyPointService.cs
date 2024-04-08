using System.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization.Conventions;
using PlantBasedPizza.OrderManager.Core.Services;
using StackExchange.Redis;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class LoyaltyPointService : ILoyaltyPointService
{
    private readonly Loyalty.LoyaltyClient _loyaltyClient;
    private readonly ILogger<LoyaltyPointService> _logger;
    private readonly IDistributedCache _distributedCache;

    public LoyaltyPointService(ILogger<LoyaltyPointService> logger, Loyalty.LoyaltyClient loyaltyClient, IDistributedCache distributedCache)
    {
        _logger = logger;
        _loyaltyClient = loyaltyClient;
        _distributedCache = distributedCache;
    }

    public async Task AddLoyaltyPoints(string customerId, string orderIdentifier, decimal orderValue)
    {
        try
        {
            var createLoyaltyPointsResult = await this._loyaltyClient.AddLoyaltyPointsAsync(
                new AddLoyaltyPointsRequest()
                {
                    CustomerIdentifier = customerId,
                    OrderIdentifier = orderIdentifier,
                    OrderValue = (double)orderValue,

                });

            if (createLoyaltyPointsResult is null)
            {
                throw new Exception("Failure sending loyalty points");
            }
        }
        catch (Exception e)
        {
            this._logger.LogInformation(e, "Failure");
            throw;
        }
    }

    public async Task<decimal> GetCustomerLoyaltyPoints(string customerId)
    {
        try
        {
            var cacheCheck = await this._distributedCache.GetStringAsync(customerId);

            if (cacheCheck != null)
            {
                Activity.Current?.AddTag("loyalty.cacheHit", true);
                
                return decimal.Parse(cacheCheck);
            }
        }
        catch (RedisServerException ex)
        {
            this._logger.LogError(ex, "Failure reading loyalty points from cache");
            
            Activity.Current?.AddTag("cache.failure", true);
        }
        
        Activity.Current?.AddTag("loyalty.cacheMiss", true);

        var loyaltyPoints = await this._loyaltyClient.GetCustomerLoyaltyPointsAsync(
            new GetCustomerLoyaltyPointsRequest()
            {
                CustomerIdentifier = customerId
            });

        await this._distributedCache.SetStringAsync(customerId, loyaltyPoints.TotalPoints.ToString("n0"));

        return Convert.ToDecimal(loyaltyPoints.TotalPoints);
    }
}

public record CreateLoyaltyPointRequest(string CustomerIdentifier, string OrderIdentifier, decimal OrderValue);