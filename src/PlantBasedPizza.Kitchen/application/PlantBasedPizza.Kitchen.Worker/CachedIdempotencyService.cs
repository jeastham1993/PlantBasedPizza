using System.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;

namespace PlantBasedPizza.Kitchen.Worker;

public class CachedIdempotencyService(IDistributedCache cache) : Idempotency
{
    public async Task<bool> HasEventBeenProcessedWithId(string eventId)
    {
        if (string.IsNullOrEmpty(eventId))
        {
            return false;
        }
        
        var cachedEvent = await cache.GetStringAsync($"events_{eventId}");

        if (cachedEvent != null)
        {
            Activity.Current?.AddTag("messaging.idempotent", "true");
            return true;
        }

        return false;
    }

    public async Task ProcessedSuccessfully(string eventId)
    {
        if (string.IsNullOrEmpty(eventId))
        {
            return;
        }
        
        await cache.SetStringAsync($"events_{eventId}", "processed", new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
        });
    }
}