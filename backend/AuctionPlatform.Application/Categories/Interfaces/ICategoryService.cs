using AuctionPlatform.Application.Categories.DTOs;

namespace AuctionPlatform.Application.Categories.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryTreeNodeDto>> GetCategoryTreeAsync(CancellationToken ct = default);
    Task<AuctionCategoryDto> GetBySlugAsync(string slug, CancellationToken ct = default);
}
