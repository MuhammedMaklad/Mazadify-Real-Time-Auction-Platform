using AuctionPlatform.Application.Common.Interfaces;
using AuctionPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatform.Infrastructure.Persistence.Repositories;

public class AuctionCategoryRepository : IAuctionCategoryRepository
{
    private readonly AppDbContext _dbContext;

    public AuctionCategoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AuctionCategory>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbContext.AuctionCategories.ToListAsync(ct);
    }

    public async Task<AuctionCategory?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.AuctionCategories.FindAsync([id], ct);
    }

    public async Task<AuctionCategory?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _dbContext.AuctionCategories
            .FirstOrDefaultAsync(c => c.Slug == slug, ct);
    }
}
