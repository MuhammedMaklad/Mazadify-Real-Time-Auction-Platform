using AuctionPlatform.Application.Auctions.DTOs;
using AuctionPlatform.Application.Common.Interfaces;
using AuctionPlatform.Application.Common.Models;
using AuctionPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatform.Infrastructure.Persistence.Repositories;

public class AuctionRepository : IAuctionRepository
{
    private readonly AppDbContext _dbContext;

    public AuctionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Auction?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Auctions.FindAsync([id], ct);
    }

    public async Task<Auction?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Auctions
            .Include(a => a.Items).ThenInclude(i => i.Images)
            .Include(a => a.Category)
            .Include(a => a.Seller)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<PagedResult<Auction>> GetFilteredAsync(
        AuctionListFilter filter, CancellationToken ct = default)
    {
        var query = _dbContext.Auctions
            .Include(a => a.Items).ThenInclude(i => i.Images)
            .Include(a => a.Category)
            .Include(a => a.Seller)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(a =>
                a.Title.ToLower().Contains(term) ||
                a.Description.ToLower().Contains(term));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(a => a.CategoryId == filter.CategoryId.Value);

        if (filter.SellerId.HasValue)
            query = query.Where(a => a.SellerId == filter.SellerId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Status) &&
            Enum.TryParse<Domain.ValueTypes.AuctionStatus>(filter.Status, ignoreCase: true, out var status))
        {
            query = query.Where(a => a.Status == status);
        }

        var totalCount = await query.CountAsync(ct);

        query = (filter.SortBy?.ToLower()) switch
        {
            "endTime" => filter.SortDescending
                ? query.OrderByDescending(a => a.EndTime)
                : query.OrderBy(a => a.EndTime),
            "currentHighestBid" => filter.SortDescending
                ? query.OrderByDescending(a => a.CurrentHighestBid)
                : query.OrderBy(a => a.CurrentHighestBid),
            "title" => filter.SortDescending
                ? query.OrderByDescending(a => a.Title)
                : query.OrderBy(a => a.Title),
            _ => filter.SortDescending
                ? query.OrderByDescending(a => a.StartTime)
                : query.OrderBy(a => a.StartTime)
        };

        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Auction>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task AddAsync(Auction auction, CancellationToken ct = default)
    {
        await _dbContext.Auctions.AddAsync(auction, ct);
    }

    public Task UpdateAsync(Auction auction, CancellationToken ct = default)
    {
        _dbContext.Auctions.Update(auction);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _dbContext.SaveChangesAsync(ct);
    }
}
