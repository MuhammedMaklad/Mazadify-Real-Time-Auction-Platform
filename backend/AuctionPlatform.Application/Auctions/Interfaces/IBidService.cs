using AuctionPlatform.Application.Auctions.DTOs;
using AuctionPlatform.Application.Common.Models;

namespace AuctionPlatform.Application.Auctions.Interfaces;

public interface IBidService
{
    /// <summary>
    /// Places a bid on an auction with full concurrency-safe validation.
    /// </summary>
    /// <param name="auctionId">The auction ID</param>
    /// <param name="bidderId">The bidder's user ID</param>
    /// <param name="request">Bid request containing amount and optional idempotency key</param>
    /// <param name="ipAddress">Client IP address for audit trail</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The placed bid DTO if successful</returns>
    Task<BidDto> PlaceBidAsync(
        Guid auctionId,
        Guid bidderId,
        PlaceBidRequest request,
        string? ipAddress = null,
        bool isAutoBid = false,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves paginated bid history for an auction.
    /// </summary>
    /// <param name="auctionId">The auction ID</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Items per page (1-100)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged result of bids</returns>
    Task<PagedResult<BidDto>> GetHistoryAsync(
        Guid auctionId,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves the highest bid for an auction.
    /// </summary>
    /// <param name="auctionId">The auction ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The highest bid DTO, or null if no bids exist</returns>
    Task<BidDto?> GetHighestAsync(
        Guid auctionId,
        CancellationToken ct = default);
}
