using AuctionPlatform.Application.AutoBids.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuctionPlatform.Application.AutoBids.Interfaces
{
    public interface IAutoBidService
    {
        Task<AutoBidDto> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<AutoBidDto> GetUserAutoBidAsync(
            Guid auctionId,
            Guid bidderId,
            CancellationToken ct = default);

        Task<AutoBidDto> CreateAsync(
            Guid auctionId,
            Guid bidderId,
            CreateAutoBidRequest request,
            CancellationToken ct = default);

        Task<AutoBidDto> UpdateAsync(
            Guid id,
            Guid bidderId,
            UpdateAutoBidRequest request,
            CancellationToken ct = default);

        Task DeleteAsync(
            Guid id,
            Guid bidderId,
            CancellationToken ct = default);
    }
}
