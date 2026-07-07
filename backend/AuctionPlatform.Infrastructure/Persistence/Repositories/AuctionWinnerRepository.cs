using AuctionPlatform.Application.Common.Interfaces;
using AuctionPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuctionPlatform.Infrastructure.Persistence.Repositories
{
    public class AuctionWinnerRepository : IAuctionWinnerRepository
    {
        private readonly AppDbContext _dbContext;

        public AuctionWinnerRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AuctionWinner?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbContext.AuctionWinners
                .Include(w => w.Auction)
                .Include(w => w.Winner)
                .Include(w => w.WinningBid)
                .FirstOrDefaultAsync(w => w.Id == id, ct);
        }

        public async Task<AuctionWinner?> GetByAuctionIdAsync(Guid auctionId, CancellationToken ct = default)
        {
            return await _dbContext.AuctionWinners
                .Include(w => w.Auction)
                .Include(w => w.Winner)
                .Include(w => w.WinningBid)
                .FirstOrDefaultAsync(w => w.AuctionId == auctionId, ct);
        }

        public async Task AddAsync(AuctionWinner winner, CancellationToken ct = default)
        {
            await _dbContext.AuctionWinners.AddAsync(winner, ct);
        }

        public Task UpdateAsync(AuctionWinner winner, CancellationToken ct = default)
        {
            _dbContext.AuctionWinners.Update(winner);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
