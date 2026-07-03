using AuctionPlatform.Application.Auctions.DTOs;
using AuctionPlatform.Application.Common.Models;
using AuctionPlatform.Domain.Entities;

namespace AuctionPlatform.Application.Common.Interfaces;

public interface IAuctionRepository
{
    Task<Auction?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Auction?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Auction>> GetFilteredAsync(AuctionListFilter filter, CancellationToken ct = default);
    Task AddAsync(Auction auction, CancellationToken ct = default);
    Task UpdateAsync(Auction auction, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
