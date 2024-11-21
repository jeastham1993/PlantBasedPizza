using System.Diagnostics;
using Grpc.Core;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class LoyaltyPointService : ILoyaltyPointService
{
    private readonly Loyalty.LoyaltyClient _loyaltyClient;
    private readonly ILogger<LoyaltyPointService> _logger;
    private readonly IDistributedCache _distributedCache;
    private readonly Metadata _metadata;

    public LoyaltyPointService(ILogger<LoyaltyPointService> logger, Loyalty.LoyaltyClient loyaltyClient, IDistributedCache distributedCache)
    {
        _logger = logger;
        _loyaltyClient = loyaltyClient;
        _distributedCache = distributedCache;
        _metadata = new Metadata()
        {
            { "dapr-app-id", "loyaltyinternal" }
        };
    }

    public async Task<decimal> GetCustomerLoyaltyPoints(string customerId)
    {
        try
        {
            var cacheCheck = await _distributedCache.GetStringAsync(customerId);

            if (cacheCheck != null)
            {
                Activity.Current?.AddTag("loyalty.cacheHit", true);
                
                return decimal.Parse(cacheCheck);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failure reading loyalty points from cache");
            
            Activity.Current?.AddTag("cache.failure", true);
        }
        
        Activity.Current?.AddTag("loyalty.cacheMiss", true);

        var loyaltyPoints = await _loyaltyClient.GetCustomerLoyaltyPointsAsync(
            new GetCustomerLoyaltyPointsRequest()
            {
                CustomerIdentifier = customerId
            }, _metadata);

        await _distributedCache.SetStringAsync(customerId, loyaltyPoints.TotalPoints.ToString("n0"));

        return Convert.ToDecimal(loyaltyPoints.TotalPoints);
    }
}