using AuctionPlatform.Domain.Entities;

namespace AuctionPlatform.Application.Common.Interfaces;

public interface IAuctionItemRepository
{
    Task<List<AuctionItem>> GetByAuctionIdAsync(Guid auctionId, CancellationToken ct = default);
}
