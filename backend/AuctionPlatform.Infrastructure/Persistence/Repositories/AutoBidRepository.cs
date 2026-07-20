using AuctionPlatform.Application.Common.Interfaces;
using AuctionPlatform.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatform.Infrastructure.Persistence.Repositories
{
    public class AutoBidRepository : IAutoBidRepository
    {
        private readonly AppDbContext _dbContext;

        public AutoBidRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AutoBid?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbContext.AutoBids
                .Include(a => a.Auction)
                .Include(a => a.Bidder)
                .FirstOrDefaultAsync(a => a.Id == id, ct);
        }

        public async Task<AutoBid?> GetByAuctionAndBidderAsync(
            Guid auctionId,
            Guid bidderId,
            CancellationToken ct = default)
        {
            return await _dbContext.AutoBids
                .FirstOrDefaultAsync(
                    a => a.AuctionId == auctionId &&
                         a.BidderId == bidderId,
                    ct);
        }

        public async Task<IEnumerable<AutoBid>> GetByAuctionAsync(
            Guid auctionId,
            CancellationToken ct = default)
        {
            return await _dbContext.AutoBids
                .Where(a => a.AuctionId == auctionId)
                .OrderByDescending(a => a.MaxAmount)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<AutoBid>> GetActiveByAuctionAsync(
            Guid auctionId,
            CancellationToken ct = default)
        {
            return await _dbContext.AutoBids
                .Where(x => x.AuctionId == auctionId &&
                            x.IsActive)
                .OrderByDescending(x => x.MaxAmount)
                .ToListAsync(ct);
        }
        public async Task AddAsync(AutoBid autoBid, CancellationToken ct = default)
        {
            await _dbContext.AutoBids.AddAsync(autoBid, ct);
        }

        public Task UpdateAsync(AutoBid autoBid, CancellationToken ct = default)
        {
            _dbContext.AutoBids.Update(autoBid);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(AutoBid autoBid, CancellationToken ct = default)
        {
            _dbContext.AutoBids.Remove(autoBid);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
