using AuctionPlatform.Application.Auctions.DTOs;
using AuctionPlatform.Application.Common.Models;

namespace AuctionPlatform.Application.Auctions.Interfaces;

public interface IAuctionService
{
    Task<AuctionDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<AuctionSummaryDto>> GetListAsync(AuctionListFilter filter, CancellationToken ct = default);
    Task<AuctionDto> CreateAsync(Guid sellerId, CreateAuctionRequest request, CancellationToken ct = default);
    Task<AuctionDto> UpdateAsync(Guid id, UpdateAuctionRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
