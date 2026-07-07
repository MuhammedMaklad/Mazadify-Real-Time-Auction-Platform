using AuctionPlatform.Application.Common.Exceptions;
using AuctionPlatform.Application.Common.Interfaces;
using AuctionPlatform.Application.Common.Models;
using AuctionPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatform.Infrastructure.Persistence.Repositories;

public class BidRepository : IBidRepository
{
    private readonly AppDbContext _dbContext;

    public BidRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Bid?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Set<Bid>()
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<Bid?> GetHighestBidAsync(Guid auctionId, CancellationToken ct = default)
    {
        return await _dbContext.Set<Bid>()
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.Amount)
            .ThenBy(b => b.PlacedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResult<Bid>> GetHistoryAsync(
        Guid auctionId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _dbContext.Set<Bid>()
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.PlacedAt)
            .AsQueryable();

        var total = await query.CountAsync(ct);

        var p = Math.Max(1, page);
        var ps = Math.Clamp(pageSize, 1, 100);

        var items = await query
            .Skip((p - 1) * ps)
            .Take(ps)
            .ToListAsync(ct);

        return new PagedResult<Bid>
        {
            Items = items,
            TotalCount = total,
            Page = p,
            PageSize = ps
        };
    }

    public async Task AddAsync(Bid bid, CancellationToken ct = default)
    {
        await _dbContext.Set<Bid>().AddAsync(bid, ct);
    }

    public Task UpdateAsync(Bid bid, CancellationToken ct = default)
    {
        _dbContext.Set<Bid>().Update(bid);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            await _dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Convert EF Core concurrency exception to application-layer exception
            throw new BidConcurrencyException(
                "Bid placement failed due to concurrent bidding activity. Please refresh the auction and try again.",
                ex);
        }
    }
}
