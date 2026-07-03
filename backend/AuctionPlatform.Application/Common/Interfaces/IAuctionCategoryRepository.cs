using AuctionPlatform.Domain.Entities;

namespace AuctionPlatform.Application.Common.Interfaces;

public interface IAuctionCategoryRepository
{
    Task<List<AuctionCategory>> GetAllAsync(CancellationToken ct = default);
    Task<AuctionCategory?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AuctionCategory?> GetBySlugAsync(string slug, CancellationToken ct = default);
}
