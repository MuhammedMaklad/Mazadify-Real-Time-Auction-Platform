using AuctionPlatform.Application.AutoBids.Interfaces;
using AuctionPlatform.Application.Common.Interfaces;
using AuctionPlatform.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuctionPlatform.Application.AutoBids.Services
{
    public class AutoBidEngine : IAutoBidEngine
    {
        private readonly IAutoBidRepository _autoBidRepository;

        public AutoBidEngine(IAutoBidRepository autoBidRepository)
        {
            _autoBidRepository = autoBidRepository;
        }

        public async Task EvaluateAsync(Bid latestBid, CancellationToken ct = default)
        {
            var autoBids = await _autoBidRepository
        .GetActiveByAuctionAsync(latestBid.AuctionId, ct);

            // Remove the bidder who placed the latest bid
            var candidates = autoBids
                .Where(x => x.BidderId != latestBid.BidderId)
                .Where(x => x.MaxAmount > latestBid.Amount)
                .OrderByDescending(x => x.MaxAmount)
                .ThenBy(x => x.CreatedAt)
                .ToList();

            if (!candidates.Any())
                return;

            // TODO:
            // Use IBidService to place automatic bids.
        }
    }
}
