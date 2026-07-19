using AuctionPlatform.Application.Common.Models;
using AuctionPlatform.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuctionPlatform.Application.Common.Interfaces
{
    public interface IBidRepository
    {
        Task<Bid?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<Bid?> GetHighestBidAsync(Guid auctionId, CancellationToken ct = default);

        Task<PagedResult<Bid>> GetHistoryAsync(
            Guid auctionId,
            int page,
            int pageSize,
            CancellationToken ct = default);

        Task AddAsync(Bid bid, CancellationToken ct = default);

        Task UpdateAsync(Bid bid, CancellationToken ct = default);

        /// <summary>
        /// Saves changes to the database.
        /// Throws BidConcurrencyException if a concurrency conflict occurs.
        /// </summary>
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
