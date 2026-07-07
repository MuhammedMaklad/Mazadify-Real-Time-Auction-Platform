using AuctionPlatform.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;

namespace AuctionPlatform.Infrastructure.Caching;

/// <summary>
/// Distributed idempotency cache using IDistributedCache (Redis or in-memory).
/// Prevents duplicate bid processing on client retries.
/// </summary>
public class DistributedIdempotencyCache : IIdempotencyCache
{
    private readonly IDistributedCache _cache;
    private const string KEY_PREFIX = "idempotency:";

    public DistributedIdempotencyCache(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<bool> IsProcessedAsync(string idempotencyKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return false;

        var cacheKey = GetCacheKey(idempotencyKey);
        var cached = await _cache.GetAsync(cacheKey, ct);
        return cached != null;
    }

    public async Task MarkAsProcessedAsync(string idempotencyKey, TimeSpan ttl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return;

        var cacheKey = GetCacheKey(idempotencyKey);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };

        await _cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes("processed"), options, ct);
    }

    private static string GetCacheKey(string idempotencyKey) 
        => $"{KEY_PREFIX}{idempotencyKey}";
}
