using System.Linq.Expressions;
using AuctionPlatform.Application.Common.Models;
using AuctionPlatform.Domain.Entities;

namespace AuctionPlatform.Application.Common.Interfaces;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Notification>> GetByUserAsync(
        Guid userId,
        bool unreadOnly,
        int page,
        int pageSize,
        CancellationToken ct = default);
    Task AddAsync(Notification notification, CancellationToken ct = default);
    Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
