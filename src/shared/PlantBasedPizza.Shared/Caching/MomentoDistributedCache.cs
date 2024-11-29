using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Momento.Sdk;
using Momento.Sdk.Responses;

namespace PlantBasedPizza.Shared.Caching;

public class MomentoDistributedCache : IDistributedCache
{
    private readonly CacheClient _cacheClient;
    private readonly string _cacheName;

    public MomentoDistributedCache(IOptions<MomentoCacheConfiguration> optionsAccessor)
    {
        _cacheClient = new CacheClient(optionsAccessor.Value.Configuration!, optionsAccessor.Value.CredentialProvider!, optionsAccessor.Value.DefaultTtl);
        _cacheName = optionsAccessor.Value.CacheName ?? "";
    }

    public byte[]? Get(string key)
    {
        return GetAsync(key).GetAwaiter().GetResult();
    }

    public async Task<byte[]?> GetAsync(string key, CancellationToken token = new CancellationToken())
    {
        var cachedData = await _cacheClient.GetAsync(_cacheName, key);

        if (cachedData is CacheGetResponse.Hit hitResponse)
        {
            return hitResponse.ValueByteArray;
        }

        return null;
    }

    public void Refresh(string key)
    {
        RefreshAsync(key).GetAwaiter().GetResult();
    }

    public async Task RefreshAsync(string key, CancellationToken token = new CancellationToken())
    {
        var cachedData = await _cacheClient.GetAsync(_cacheName, key);

        if (cachedData is CacheGetResponse.Error || cachedData is CacheGetResponse.Miss)
        {
            return;
        }

        var cacheHitData = cachedData as CacheGetResponse.Hit;
        
        if (cacheHitData is null)
        {
            return;
        }

        await _cacheClient.SetAsync(_cacheName, key, cacheHitData.ValueByteArray);
    }

    public void Remove(string key)
    {
        RemoveAsync(key).GetAwaiter().GetResult();
    }

    public async Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
    {
        await _cacheClient.DeleteAsync(_cacheName, key);
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        SetAsync(key, value, options).GetAwaiter().GetResult();
    }

    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
        CancellationToken token = new CancellationToken())
    {
        await _cacheClient.SetAsync(_cacheName, key, value, options.SlidingExpiration);
    }
}