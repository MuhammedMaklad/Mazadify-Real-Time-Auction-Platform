using AuctionPlatform.Application.AutoBids.Interfaces;
using AuctionPlatform.Application.Common.Interfaces;
using AuctionPlatform.Domain.Entities;
using AuctionPlatform.Application.Auctions.Interfaces;
using AuctionPlatform.Application.Auctions.DTOs;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AuctionPlatform.Application.AutoBids.Services
{
    public class AutoBidEngine : IAutoBidEngine
    {
        private readonly IAutoBidRepository _autoBidRepository;
        private readonly IAuctionRepository _auctionRepository;
        private readonly IBidRepository _bidRepository;
        private readonly IServiceProvider _serviceProvider;

        public AutoBidEngine(
            IAutoBidRepository autoBidRepository,
            IAuctionRepository auctionRepository,
            IBidRepository bidRepository,
            IServiceProvider serviceProvider)
        {
            _autoBidRepository = autoBidRepository;
            _auctionRepository = auctionRepository;
            _bidRepository = bidRepository;
            _serviceProvider = serviceProvider;
        }

        public async Task EvaluateAsync(Bid latestBid, CancellationToken ct = default)
        {
            var auction = await _auctionRepository.GetByIdAsync(latestBid.AuctionId, ct);
            if (auction == null)
                return;

            var bidService = _serviceProvider.GetRequiredService<IBidService>();

            while (true)
            {
                // 1. Fetch all active auto-bids for this auction.
                var autoBids = await _autoBidRepository.GetActiveByAuctionAsync(latestBid.AuctionId, ct);

                // 2. Select the candidate with the highest MaxAmount who isn't the current winner (latestBid.BidderId).
                var candidate = autoBids
                    .Where(x => x.BidderId != latestBid.BidderId)
                    .OrderByDescending(x => x.MaxAmount)
                    .ThenBy(x => x.CreatedAt)
                    .FirstOrDefault();

                if (candidate == null)
                    break; // No other active auto-bids

                // Calculate the next bid amount
                var nextAmount = latestBid.Amount + auction.BidIncrement;

                // Deactivate the auto-bid if it cannot meet the increment
                if (candidate.MaxAmount < nextAmount)
                {
                    candidate.IsActive = false;
                    await _autoBidRepository.UpdateAsync(candidate, ct);
                    await _autoBidRepository.SaveChangesAsync(ct);
                    continue; // Check if there are other candidates
                }

                // Place the auto-bid
                var placeBidRequest = new PlaceBidRequest
                {
                    Amount = nextAmount,
                    IdempotencyKey = Guid.NewGuid().ToString()
                };

                // Place automatic bid through BidService
                var result = await bidService.PlaceBidAsync(
                    latestBid.AuctionId,
                    candidate.BidderId,
                    placeBidRequest,
                    ipAddress: null,
                    isAutoBid: true,
                    ct: ct
                );

                var newLatestBid = await _bidRepository.GetByIdAsync(result.Id, ct);
                if (newLatestBid == null)
                    break;

                latestBid = newLatestBid;
            }
        }
    }
}
