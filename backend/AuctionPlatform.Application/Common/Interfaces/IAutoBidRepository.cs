using AuctionPlatform.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuctionPlatform.Application.Common.Interfaces
{
    public interface IAutoBidRepository
    {
        Task<AutoBid?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default);

        Task<AutoBid?> GetByAuctionAndBidderAsync(
            Guid auctionId,
            Guid bidderId,
            CancellationToken ct = default);

        Task<IEnumerable<AutoBid>> GetByAuctionAsync(
            Guid auctionId,
            CancellationToken ct = default);

        Task<IEnumerable<AutoBid>> GetActiveByAuctionAsync(
            Guid auctionId,
            CancellationToken ct = default);

        Task AddAsync(
            AutoBid autoBid,
            CancellationToken ct = default);

        Task UpdateAsync(
            AutoBid autoBid,
            CancellationToken ct = default);

        Task DeleteAsync(
            AutoBid autoBid,
            CancellationToken ct = default);

        Task SaveChangesAsync(
            CancellationToken ct = default);
    }
}
