using AuctionPlatform.Application.Auctions.DTOs;
using AuctionPlatform.Application.Auctions.Interfaces;
using AuctionPlatform.Application.Common.Exceptions;
using AuctionPlatform.Application.Common.Interfaces;
using AuctionPlatform.Application.Common.Models;
using AuctionPlatform.Domain.Entities;
using AuctionPlatform.Domain.ValueTypes;
using AutoMapper;

namespace AuctionPlatform.Application.Auctions.Services;

public class BidService : IBidService
{
    private readonly IBidRepository _bidRepository;
    private readonly IAuctionRepository _auctionRepository;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyCache _idempotencyCache;
    private readonly IBidAuditService _auditService;
    private const int IDEMPOTENCY_TTL_MINUTES = 10;

    public BidService(
        IBidRepository bidRepository,
        IAuctionRepository auctionRepository,
        IMapper mapper,
        INotificationService notificationService,
        IIdempotencyCache idempotencyCache,
        IBidAuditService auditService)
    {
        _bidRepository = bidRepository;
        _auctionRepository = auctionRepository;
        _mapper = mapper;
        _notificationService = notificationService;
        _idempotencyCache = idempotencyCache;
        _auditService = auditService;
    }

    /// <summary>
    /// Places a bid on an auction with full concurrency-safe validation.
    /// Version 4: Includes transaction, previous bid update, and optimistic concurrency handling.
    /// </summary>
    public async Task<BidDto> PlaceBidAsync(
        Guid auctionId,
        Guid bidderId,
        PlaceBidRequest request,
        string? ipAddress = null,
        bool isAutoBid = false,
        CancellationToken ct = default)
    {
        // Check idempotency: prevent duplicate bid if client retries
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            if (await _idempotencyCache.IsProcessedAsync(request.IdempotencyKey, ct))
                throw new InvalidOperationException(
                    "This bid has already been processed. Please use a new idempotency key.");
        }

        // Load auction
        var auction = await _auctionRepository.GetByIdAsync(auctionId, ct);
        if (auction is null)
            throw new KeyNotFoundException($"Auction with ID {auctionId} not found.");

        // Validate auction status (must be Live)
        if (auction.Status != AuctionStatus.Live)
            throw new InvalidOperationException(
                $"Cannot place bid on auction with status '{auction.Status}'. Auction must be Live.");

        // Validate times
        var now = DateTime.UtcNow;
        if (now < auction.StartTime || now > auction.EndTime)
            throw new InvalidOperationException(
                "Bid is outside auction time window.");

        // Validate bidder is not the seller
        if (bidderId == auction.SellerId)
            throw new InvalidOperationException(
                "Seller cannot place bids on their own auction.");

        // Validate bid amount meets minimum requirement
        var minimumBid = auction.CurrentHighestBid > 0
            ? auction.CurrentHighestBid + auction.BidIncrement
            : auction.StartingPrice;

        if (request.Amount < minimumBid)
            throw new InvalidOperationException(
                $"Bid amount must be at least {minimumBid:C}. Current highest bid is {auction.CurrentHighestBid:C}.");

        // Load current highest bid (to mark as Outbid)
        var previousHighestBid = await _bidRepository.GetHighestBidAsync(auctionId, ct);

        // Create new bid
        var bid = new Bid
        {
            Id = Guid.NewGuid(),
            AuctionId = auctionId,
            BidderId = bidderId,
            Amount = request.Amount,
            Status = BidStatus.Accepted,
            PlacedAt = DateTime.UtcNow,
            IpAddress = ipAddress,
            IsAutoBid = isAutoBid
        };

        try
        {
            // Begin transaction: insert bid, update auction, mark previous bid as Outbid
            await _bidRepository.AddAsync(bid, ct);

            // If there was a previous highest bid, mark it as Outbid
            if (previousHighestBid is not null && previousHighestBid.Amount < request.Amount)
            {
                previousHighestBid.Status = BidStatus.Outbid;
                await _bidRepository.UpdateAsync(previousHighestBid, ct);
            }

            // Update auction's current highest bid
            auction.CurrentHighestBid = request.Amount;
            await _auctionRepository.UpdateAsync(auction, ct);

            // SaveChanges is now atomic within the transaction
            // BidConcurrencyException will be thrown here if a concurrency conflict occurs
            await _bidRepository.SaveChangesAsync(ct);

            // ======================================================================
            // TODO (Member 5 - AutoBid Engine)
            //
            // After a successful manual bid, trigger the AutoBid Engine.
            //
            // The engine will:
            // 1. Load all active auto bids for this auction.
            // 2. Exclude the latest bidder.
            // 3. Select the highest eligible auto bid.
            // 4. Calculate the next bid amount.
            // 5. Place an automatic bid through IBidService.
            // 6. Repeat until no bidder can outbid.
            //
            // Example:
            //
            // await _autoBidEngine.EvaluateAsync(bid, ct);
            //
            // ======================================================================
        }
        catch (BidConcurrencyException)
        {
            // Re-throw: let the controller map this to 409 Conflict
            throw;
        }
        catch (Exception)
        {
            // Transaction will rollback automatically on exception
            throw;
        }

        // Mark idempotency key as processed AFTER successful commit
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            await _idempotencyCache.MarkAsProcessedAsync(
                request.IdempotencyKey, 
                TimeSpan.FromMinutes(IDEMPOTENCY_TTL_MINUTES),
                ct);
        }

        // Audit log: bid placed
        await _auditService.LogBidPlacedAsync(auctionId, bidderId, request.Amount, ipAddress, request.IdempotencyKey, ct);

        // Notify after successful commit
        await _notificationService.NotifyBidPlacedAsync(auctionId, bidderId, request.Amount, ct);

        // If there was a previous highest bidder, notify them of being outbid
        if (previousHighestBid is not null && previousHighestBid.Amount < request.Amount)
        {
            // Audit log: bid outbid
            await _auditService.LogBidOutbidAsync(auctionId, previousHighestBid.BidderId, request.Amount, ipAddress, ct);

            await _notificationService.NotifyOutbidAsync(
                auctionId,
                previousHighestBid.BidderId,
                request.Amount,
                ct);
        }

        // Map and return
        return _mapper.Map<BidDto>(bid);
    }

    /// <summary>
    /// Retrieves paginated bid history for an auction.
    /// </summary>
    public async Task<PagedResult<BidDto>> GetHistoryAsync(
        Guid auctionId,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _bidRepository.GetHistoryAsync(auctionId, page, pageSize, ct);

        return new PagedResult<BidDto>
        {
            Items = _mapper.Map<List<BidDto>>(result.Items),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    /// <summary>
    /// Retrieves the highest bid for an auction.
    /// </summary>
    public async Task<BidDto?> GetHighestAsync(
        Guid auctionId,
        CancellationToken ct = default)
    {
        var bid = await _bidRepository.GetHighestBidAsync(auctionId, ct);
        if (bid is null)
            return null;

        return _mapper.Map<BidDto>(bid);
    }
}
