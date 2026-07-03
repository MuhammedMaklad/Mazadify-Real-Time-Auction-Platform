using AuctionPlatform.Application.Common.Interfaces;
using AuctionPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatform.Infrastructure.Persistence.Repositories;

public class AuctionItemRepository : IAuctionItemRepository
{
    private readonly AppDbContext _dbContext;

    public AuctionItemRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AuctionItem>> GetByAuctionIdAsync(Guid auctionId, CancellationToken ct = default)
    {
        return await _dbContext.AuctionItems
            .Include(i => i.Images)
            .Where(i => i.AuctionId == auctionId)
            .ToListAsync(ct);
    }
}
