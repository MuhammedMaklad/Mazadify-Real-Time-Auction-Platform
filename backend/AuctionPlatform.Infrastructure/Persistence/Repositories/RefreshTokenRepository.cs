using AuctionPlatform.Application.Common.Interfaces;
using AuctionPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatform.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _dbContext;

    public RefreshTokenRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        await _dbContext.RefreshTokens.AddAsync(token, ct);
    }

    public async Task<RefreshToken?> GetActiveByTokenAsync(string token, CancellationToken ct = default)
    {
        return await _dbContext.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(
                t => t.Token == token && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow,
                ct);
    }

    public Task RevokeAsync(RefreshToken token, string? replacedByToken = null, CancellationToken ct = default)
    {
        token.IsRevoked = true;
        token.ReplacedByToken = replacedByToken;
        return Task.CompletedTask;
    }

    public async Task RevokeAllByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var tokens = await _dbContext.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync(ct);

        foreach (var t in tokens)
            t.IsRevoked = true;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _dbContext.SaveChangesAsync(ct);
    }
}
