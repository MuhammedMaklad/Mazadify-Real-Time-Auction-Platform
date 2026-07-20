using AuctionPlatform.Application.Winners.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuctionPlatform.Application.Winners.Interfaces
{
    public interface IAuctionWinnerService
    {
        Task<AuctionWinnerDto?> SettleAuctionAsync(Guid auctionId, CancellationToken ct = default);
        Task<AuctionWinnerDto> GetWinnerByAuctionIdAsync(Guid auctionId, CancellationToken ct = default);
        Task<AuctionWinnerDto> UpdateDeliveryStatusAsync(Guid auctionId, UpdateDeliveryStatusRequest request, CancellationToken ct = default);
        Task<AuctionWinnerDto> UpdateTrackingNumberAsync(Guid auctionId, UpdateTrackingNumberRequest request, CancellationToken ct = default);
    }
}
