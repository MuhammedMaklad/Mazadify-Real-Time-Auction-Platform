using AuctionPlatform.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuctionPlatform.Application.Common.Interfaces
{
    public interface IAuctionWinnerRepository
    {
        Task<AuctionWinner?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<AuctionWinner?> GetByAuctionIdAsync(Guid auctionId, CancellationToken ct = default);
        Task AddAsync(AuctionWinner winner, CancellationToken ct = default);
        Task UpdateAsync(AuctionWinner winner, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
