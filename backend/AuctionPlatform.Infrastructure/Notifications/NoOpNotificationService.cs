using AuctionPlatform.Application.Common.Interfaces;
using AuctionPlatform.Application.Common.Models;
using AuctionPlatform.Application.Notifications.DTOs;

namespace AuctionPlatform.Infrastructure.Notifications;

/// <summary>
/// Temporary no-op notification service.
/// Will be replaced with SignalR implementation in the Real-Time module.
/// </summary>
public class NoOpNotificationService : INotificationService
{
    public Task NotifyBidPlacedAsync(
        Guid auctionId,
        Guid bidderId,
        decimal amount,
        CancellationToken ct = default)
    {
        // TODO: Implement real SignalR notification
        return Task.CompletedTask;
    }

    public Task NotifyOutbidAsync(
        Guid auctionId,
        Guid previousBidderId,
        decimal currentHighestBid,
        CancellationToken ct = default)
    {
        // TODO: Implement real SignalR notification
        return Task.CompletedTask;
    }

    public Task NotifyYouWonAsync(
        Guid auctionId,
        Guid winnerId,
        decimal finalPrice,
        CancellationToken ct = default)
    {
        // TODO: Implement real SignalR notification
        return Task.CompletedTask;
    }

    public Task NotifyPaymentRequiredAsync(
        Guid auctionId,
        Guid winnerId,
        decimal finalPrice,
        CancellationToken ct = default)
    {
        // TODO: Implement real SignalR notification
        return Task.CompletedTask;
    }

    public Task<PagedResult<NotificationDto>> GetUserNotificationsAsync(
        Guid userId,
        bool unreadOnly,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        return Task.FromResult(new PagedResult<NotificationDto>
        {
            Items = new List<NotificationDto>(),
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        });
    }

    public Task MarkAsReadAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}
