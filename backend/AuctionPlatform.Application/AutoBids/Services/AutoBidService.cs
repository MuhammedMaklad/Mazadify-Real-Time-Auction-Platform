using AuctionPlatform.Application.AutoBids.DTOs;
using AuctionPlatform.Application.AutoBids.Interfaces;
using AuctionPlatform.Application.Common.Interfaces;
using AuctionPlatform.Domain.Entities;
using AuctionPlatform.Domain.ValueTypes;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuctionPlatform.Application.AutoBids.Services
{
    public class AutoBidService : IAutoBidService
    {
        private readonly IAutoBidRepository _autoBidRepository;
        private readonly IAuctionRepository _auctionRepository;
        private readonly IMapper _mapper;

        public AutoBidService(
            IAutoBidRepository autoBidRepository,
            IAuctionRepository auctionRepository,
            IMapper mapper)
        {
            _autoBidRepository = autoBidRepository;
            _auctionRepository = auctionRepository;
            _mapper = mapper;
        }

        public async Task<AutoBidDto> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var autoBid = await _autoBidRepository.GetByIdAsync(id, ct);

            if (autoBid is null)
                throw new KeyNotFoundException($"Auto bid with ID {id} not found.");

            return _mapper.Map<AutoBidDto>(autoBid);
        }

        public async Task<AutoBidDto> GetUserAutoBidAsync(
            Guid auctionId,
            Guid bidderId,
            CancellationToken ct = default)
        {
            var autoBid = await _autoBidRepository
                .GetByAuctionAndBidderAsync(auctionId, bidderId, ct);

            if (autoBid is null)
                throw new KeyNotFoundException($"Auto bid for auction {auctionId} was not found.");

            return _mapper.Map<AutoBidDto>(autoBid);
        }

        public async Task<AutoBidDto> CreateAsync(
            Guid auctionId,
            Guid bidderId,
            CreateAutoBidRequest request,
            CancellationToken ct = default)
        {
            var auction = await _auctionRepository.GetByIdAsync(auctionId, ct);

            if (auction is null)
                throw new KeyNotFoundException($"Auction with ID {auctionId} not found.");

            if (auction.Status != AuctionStatus.Live)
                throw new InvalidOperationException(
                    "Auto bids can only be created for live auctions.");

            var existing = await _autoBidRepository
                .GetByAuctionAndBidderAsync(auctionId, bidderId, ct);

            if (existing is not null)
                throw new InvalidOperationException(
                    "You already have an auto bid for this auction.");

            if (request.MaxAmount <= auction.CurrentHighestBid)
                throw new InvalidOperationException(
                    "Maximum amount must be greater than current highest bid.");

            var autoBid = new AutoBid
            {
                Id = Guid.NewGuid(),
                AuctionId = auctionId,
                BidderId = bidderId,
                MaxAmount = request.MaxAmount,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _autoBidRepository.AddAsync(autoBid, ct);
            await _autoBidRepository.SaveChangesAsync(ct);

            return _mapper.Map<AutoBidDto>(autoBid);
        }

        public async Task<AutoBidDto> UpdateAsync(
    Guid id,
    Guid bidderId,
    UpdateAutoBidRequest request,
    CancellationToken ct = default)
        {
            var autoBid = await _autoBidRepository.GetByIdAsync(id, ct);

            if (autoBid is null)
                throw new KeyNotFoundException($"Auto bid with ID {id} not found.");

            if (autoBid.BidderId != bidderId)
                throw new UnauthorizedAccessException(
                    "You are not allowed to update this auto bid.");

            if (request.MaxAmount <= autoBid.Auction.CurrentHighestBid)
                throw new InvalidOperationException(
                    "Maximum amount must be greater than the current highest bid.");

            autoBid.MaxAmount = request.MaxAmount;
            autoBid.IsActive = request.IsActive;
            autoBid.UpdatedAt = DateTime.UtcNow;

            await _autoBidRepository.UpdateAsync(autoBid, ct);
            await _autoBidRepository.SaveChangesAsync(ct);

            return _mapper.Map<AutoBidDto>(autoBid);
        }

        public async Task DeleteAsync(
    Guid id,
    Guid bidderId,
    CancellationToken ct = default)
        {
            var autoBid = await _autoBidRepository.GetByIdAsync(id, ct);

            if (autoBid is null)
                throw new KeyNotFoundException($"Auto bid with ID {id} not found.");

            if (autoBid.BidderId != bidderId)
                throw new UnauthorizedAccessException(
                    "You are not allowed to delete this auto bid.");

            await _autoBidRepository.DeleteAsync(autoBid, ct);
            await _autoBidRepository.SaveChangesAsync(ct);
        }
    }
}
