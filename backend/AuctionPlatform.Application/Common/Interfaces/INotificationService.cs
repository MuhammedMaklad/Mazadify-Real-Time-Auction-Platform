namespace AuctionPlatform.Application.Common.Interfaces;

/// <summary>
/// Infrastructure-agnostic notification service.
/// Implementation will handle SignalR broadcast and persistence.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Notifies all viewers of an auction that a new bid has been placed.
    /// </summary>
    Task NotifyBidPlacedAsync(
        Guid auctionId,
        Guid bidderId,
        decimal amount,
        CancellationToken ct = default);

    /// <summary>
    /// Notifies a bidder that they have been outbid.
    /// </summary>
    Task NotifyOutbidAsync(
        Guid auctionId,
        Guid previousBidderId,
        decimal currentHighestBid,
        CancellationToken ct = default);

    /// <summary>
    /// Notifies the winner of the auction.
    /// </summary>
    Task NotifyYouWonAsync(
        Guid auctionId,
        Guid winnerId,
        decimal finalPrice,
        CancellationToken ct = default);

    /// <summary>
    /// Notifies the winner that payment is required.
    /// </summary>
    Task NotifyPaymentRequiredAsync(
        Guid auctionId,
        Guid winnerId,
        decimal finalPrice,
        CancellationToken ct = default);
}
