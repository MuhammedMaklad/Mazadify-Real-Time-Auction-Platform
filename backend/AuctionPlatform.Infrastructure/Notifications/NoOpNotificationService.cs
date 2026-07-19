using AuctionPlatform.Application.Common.Interfaces;

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
}
