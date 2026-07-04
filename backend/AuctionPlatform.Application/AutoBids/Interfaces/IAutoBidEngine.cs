using AuctionPlatform.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuctionPlatform.Application.AutoBids.Interfaces
{
    public interface IAutoBidEngine
    {
        Task EvaluateAsync(Bid latestBid, CancellationToken ct = default);
    }
}
