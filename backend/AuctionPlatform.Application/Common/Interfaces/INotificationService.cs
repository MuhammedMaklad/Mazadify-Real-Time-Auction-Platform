using AuctionPlatform.Application.Common.Models;
using AuctionPlatform.Application.Notifications.DTOs;

namespace AuctionPlatform.Application.Common.Interfaces;

public interface INotificationService
{
    Task<NotificationDto> CreateAndBroadcastAsync(
        CreateNotificationRequest request,
        CancellationToken ct = default);

    Task BroadcastAsync(
        string eventName,
        object payload,
        BroadcastTarget target,
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

    Task MarkAllAsReadAsync(
        Guid userId,
        CancellationToken ct = default);
}
