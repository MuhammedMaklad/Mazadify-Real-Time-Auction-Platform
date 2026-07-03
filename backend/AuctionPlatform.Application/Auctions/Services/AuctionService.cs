using AuctionPlatform.Application.Auctions.DTOs;
using AuctionPlatform.Application.Auctions.Interfaces;
using AuctionPlatform.Application.Common.Interfaces;
using AuctionPlatform.Application.Common.Models;
using AuctionPlatform.Domain.Entities;
using AuctionPlatform.Domain.ValueTypes;
using AutoMapper;

namespace AuctionPlatform.Application.Auctions.Services;

public class AuctionService : IAuctionService
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IAuctionCategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public AuctionService(
        IAuctionRepository auctionRepository,
        IAuctionCategoryRepository categoryRepository,
        IMapper mapper)
    {
        _auctionRepository = auctionRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<AuctionDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var auction = await _auctionRepository.GetByIdWithItemsAsync(id, ct);
        if (auction is null)
            throw new KeyNotFoundException($"Auction with ID {id} not found.");

        return _mapper.Map<AuctionDto>(auction);
    }

    public async Task<PagedResult<AuctionSummaryDto>> GetListAsync(
        AuctionListFilter filter, CancellationToken ct = default)
    {
        var paged = await _auctionRepository.GetFilteredAsync(filter, ct);

        return new PagedResult<AuctionSummaryDto>
        {
            Items = _mapper.Map<List<AuctionSummaryDto>>(paged.Items),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<AuctionDto> CreateAsync(
        Guid sellerId, CreateAuctionRequest request, CancellationToken ct = default)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, ct);
        if (category is null)
            throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found.");

        var auction = new Auction
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            CategoryId = request.CategoryId,
            Title = request.Title,
            Description = request.Description,
            StartingPrice = request.StartingPrice,
            ReservePrice = request.ReservePrice,
            CurrentHighestBid = 0,
            BidIncrement = request.BidIncrement,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = AuctionStatus.Draft,
            DeliveryType = Enum.Parse<DeliveryType>(request.DeliveryType),
            DeliveryNotes = request.DeliveryNotes,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var itemRequest in request.Items)
        {
            var item = new AuctionItem
            {
                Id = Guid.NewGuid(),
                AuctionId = auction.Id,
                Name = itemRequest.Name,
                Description = itemRequest.Description,
                Condition = itemRequest.Condition,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var imageUrl in itemRequest.ImageUrls)
            {
                item.Images.Add(new AuctionItemImage
                {
                    Id = Guid.NewGuid(),
                    AuctionItemId = item.Id,
                    ImageUrl = imageUrl,
                    IsPrimary = item.Images.Count == 0,
                });
            }

            auction.Items.Add(item);
        }

        await _auctionRepository.AddAsync(auction, ct);
        await _auctionRepository.SaveChangesAsync(ct);

        return _mapper.Map<AuctionDto>(auction);
    }

    public async Task<AuctionDto> UpdateAsync(
        Guid id, UpdateAuctionRequest request, CancellationToken ct = default)
    {
        var auction = await _auctionRepository.GetByIdAsync(id, ct);
        if (auction is null)
            throw new KeyNotFoundException($"Auction with ID {id} not found.");

        if (auction.Status != AuctionStatus.Draft && auction.Status != AuctionStatus.Scheduled)
            throw new InvalidOperationException(
                "Only auctions in Draft or Scheduled status can be updated.");

        if (request.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value, ct);
            if (category is null)
                throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found.");
            auction.CategoryId = request.CategoryId.Value;
        }

        if (request.Title is not null)
            auction.Title = request.Title;

        if (request.Description is not null)
            auction.Description = request.Description;

        if (request.ReservePrice.HasValue)
            auction.ReservePrice = request.ReservePrice.Value;

        if (request.BidIncrement.HasValue)
            auction.BidIncrement = request.BidIncrement.Value;

        if (request.StartTime.HasValue)
            auction.StartTime = request.StartTime.Value;

        if (request.EndTime.HasValue)
            auction.EndTime = request.EndTime.Value;

        if (request.DeliveryType is not null)
            auction.DeliveryType = Enum.Parse<DeliveryType>(request.DeliveryType);

        if (request.DeliveryNotes is not null)
            auction.DeliveryNotes = request.DeliveryNotes;

        auction.UpdatedAt = DateTime.UtcNow;

        await _auctionRepository.UpdateAsync(auction, ct);
        await _auctionRepository.SaveChangesAsync(ct);

        return _mapper.Map<AuctionDto>(auction);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var auction = await _auctionRepository.GetByIdAsync(id, ct);
        if (auction is null)
            throw new KeyNotFoundException($"Auction with ID {id} not found.");

        if (auction.Status != AuctionStatus.Draft)
            throw new InvalidOperationException("Only auctions in Draft status can be deleted.");

        auction.IsDeleted = true;
        auction.DeletedAt = DateTime.UtcNow;

        await _auctionRepository.UpdateAsync(auction, ct);
        await _auctionRepository.SaveChangesAsync(ct);
    }
}
