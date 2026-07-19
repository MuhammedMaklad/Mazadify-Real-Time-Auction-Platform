namespace AuctionPlatform.Application.Common.Interfaces;

/// <summary>
/// Caches idempotency keys to prevent duplicate bid processing on client retries.
/// Short-lived cache (typically 5-10 minutes).
/// </summary>
public interface IIdempotencyCache
{
    /// <summary>
    /// Checks if an idempotency key has already been processed.
    /// </summary>
    /// <param name="idempotencyKey">Client-generated unique identifier</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if already processed, false if new</returns>
    Task<bool> IsProcessedAsync(string idempotencyKey, CancellationToken ct = default);

    /// <summary>
    /// Marks an idempotency key as processed.
    /// </summary>
    /// <param name="idempotencyKey">Client-generated unique identifier</param>
    /// <param name="ttl">Time-to-live for cache entry (e.g., 10 minutes)</param>
    /// <param name="ct">Cancellation token</param>
    Task MarkAsProcessedAsync(string idempotencyKey, TimeSpan ttl, CancellationToken ct = default);
}
