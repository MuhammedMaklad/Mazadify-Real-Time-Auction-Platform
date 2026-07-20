using AuctionPlatform.Application.Common.Models;
using AuctionPlatform.Application.Notifications.DTOs;

namespace AuctionPlatform.Application.Common.Interfaces;

/// <summary>
/// Infrastructure-agnostic notification service.
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

    Task<PagedResult<NotificationDto>> GetUserNotificationsAsync(
        Guid userId,
        bool unreadOnly,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task MarkAsReadAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken ct = default);

    Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
}
