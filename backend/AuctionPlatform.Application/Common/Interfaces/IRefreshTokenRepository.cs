using AuctionPlatform.Domain.Entities;

namespace AuctionPlatform.Application.Common.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken ct = default);
    Task<RefreshToken?> GetActiveByTokenAsync(string token, CancellationToken ct = default);
    Task RevokeAsync(RefreshToken token, string? replacedByToken = null, CancellationToken ct = default);
    Task RevokeAllByUserAsync(Guid userId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
