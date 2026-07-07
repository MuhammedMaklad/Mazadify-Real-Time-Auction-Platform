using AuctionPlatform.Application.Common.Interfaces;
using AuctionPlatform.Application.Winners.DTOs;
using AuctionPlatform.Application.Winners.Interfaces;
using AuctionPlatform.Domain.Entities;
using AuctionPlatform.Domain.ValueTypes;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuctionPlatform.Application.Winners.Services
{
    public class AuctionWinnerService : IAuctionWinnerService
    {
        private readonly IAuctionWinnerRepository _winnerRepository;
        private readonly IAuctionRepository _auctionRepository;
        private readonly IBidRepository _bidRepository;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;

        public AuctionWinnerService(
            IAuctionWinnerRepository winnerRepository,
            IAuctionRepository auctionRepository,
            IBidRepository bidRepository,
            INotificationService notificationService,
            IMapper mapper)
        {
            _winnerRepository = winnerRepository;
            _auctionRepository = auctionRepository;
            _bidRepository = bidRepository;
            _notificationService = notificationService;
            _mapper = mapper;
        }

        public async Task<AuctionWinnerDto?> SettleAuctionAsync(Guid auctionId, CancellationToken ct = default)
        {
            var auction = await _auctionRepository.GetByIdAsync(auctionId, ct);
            if (auction is null)
                throw new KeyNotFoundException($"Auction with ID {auctionId} not found.");

            // Check if already settled
            var existingWinner = await _winnerRepository.GetByAuctionIdAsync(auctionId, ct);
            if (existingWinner is not null)
                return _mapper.Map<AuctionWinnerDto>(existingWinner);

            if (auction.Status == AuctionStatus.Cancelled || auction.Status == AuctionStatus.ReserveNotMet)
                throw new InvalidOperationException($"Cannot settle auction in status '{auction.Status}'.");

            // Ensure auction is ended (either status is Ended, or EndTime has passed)
            if (auction.Status != AuctionStatus.Ended && DateTime.UtcNow < auction.EndTime)
                throw new InvalidOperationException("Cannot settle a live auction before its end time.");

            var highestBid = await _bidRepository.GetHighestBidAsync(auctionId, ct);

            // If no bids exist or reserve price is not met
            if (highestBid is null || auction.CurrentHighestBid < auction.ReservePrice)
            {
                auction.Status = AuctionStatus.ReserveNotMet;
                await _auctionRepository.UpdateAsync(auction, ct);
                await _auctionRepository.SaveChangesAsync(ct);
                return null;
            }

            // Create winner
            var winner = new AuctionWinner
            {
                Id = Guid.NewGuid(),
                AuctionId = auctionId,
                WinnerId = highestBid.BidderId,
                WinningBidId = highestBid.Id,
                FinalPrice = highestBid.Amount,
                ShippingCost = 0, // default
                PaymentStatus = PaymentStatus.Pending,
                DeliveryStatus = DeliveryStatus.Pending,
                AwardedAt = DateTime.UtcNow
            };

            // Update Auction Status to Ended
            auction.Status = AuctionStatus.Ended;
            await _auctionRepository.UpdateAsync(auction, ct);

            // Update Bid Status to Won
            highestBid.Status = BidStatus.Won;
            await _bidRepository.UpdateAsync(highestBid, ct);

            await _winnerRepository.AddAsync(winner, ct);
            await _winnerRepository.SaveChangesAsync(ct);
            await _auctionRepository.SaveChangesAsync(ct);

            // Trigger notifications
            await _notificationService.NotifyYouWonAsync(auctionId, winner.WinnerId, winner.FinalPrice, ct);
            await _notificationService.NotifyPaymentRequiredAsync(auctionId, winner.WinnerId, winner.FinalPrice, ct);

            return _mapper.Map<AuctionWinnerDto>(winner);
        }

        public async Task<AuctionWinnerDto> GetWinnerByAuctionIdAsync(Guid auctionId, CancellationToken ct = default)
        {
            var winner = await _winnerRepository.GetByAuctionIdAsync(auctionId, ct);
            if (winner is null)
                throw new KeyNotFoundException($"Winner for auction {auctionId} not found.");

            return _mapper.Map<AuctionWinnerDto>(winner);
        }

        public async Task<AuctionWinnerDto> UpdateDeliveryStatusAsync(
            Guid auctionId,
            UpdateDeliveryStatusRequest request,
            CancellationToken ct = default)
        {
            var winner = await _winnerRepository.GetByAuctionIdAsync(auctionId, ct);
            if (winner is null)
                throw new KeyNotFoundException($"Winner for auction {auctionId} not found.");

            if (!Enum.TryParse<DeliveryStatus>(request.Status, true, out var newStatus))
                throw new ArgumentException($"Invalid delivery status '{request.Status}'.");

            // Enforce state machine transitions: Pending -> Shipped -> Delivered/PickedUp, Pending -> PickedUp, Pending/Shipped -> Cancelled
            var currentStatus = winner.DeliveryStatus;
            bool isValid = false;

            if (currentStatus == DeliveryStatus.Pending)
            {
                isValid = newStatus == DeliveryStatus.Shipped ||
                          newStatus == DeliveryStatus.PickedUp ||
                          newStatus == DeliveryStatus.Cancelled;
            }
            else if (currentStatus == DeliveryStatus.Shipped)
            {
                isValid = newStatus == DeliveryStatus.Delivered ||
                          newStatus == DeliveryStatus.Cancelled;
            }

            if (!isValid)
                throw new InvalidOperationException($"Invalid delivery status transition from '{currentStatus}' to '{newStatus}'.");

            winner.DeliveryStatus = newStatus;
            await _winnerRepository.UpdateAsync(winner, ct);
            await _winnerRepository.SaveChangesAsync(ct);

            return _mapper.Map<AuctionWinnerDto>(winner);
        }

        public async Task<AuctionWinnerDto> UpdateTrackingNumberAsync(
            Guid auctionId,
            UpdateTrackingNumberRequest request,
            CancellationToken ct = default)
        {
            var winner = await _winnerRepository.GetByAuctionIdAsync(auctionId, ct);
            if (winner is null)
                throw new KeyNotFoundException($"Winner for auction {auctionId} not found.");

            winner.TrackingNumber = request.TrackingNumber;
            await _winnerRepository.UpdateAsync(winner, ct);
            await _winnerRepository.SaveChangesAsync(ct);

            return _mapper.Map<AuctionWinnerDto>(winner);
        }
    }
}
