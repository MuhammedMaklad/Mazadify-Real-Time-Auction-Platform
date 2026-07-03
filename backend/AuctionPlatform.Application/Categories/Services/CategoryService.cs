using AuctionPlatform.Application.Categories.DTOs;
using AuctionPlatform.Application.Categories.Interfaces;
using AuctionPlatform.Application.Common.Interfaces;
using AutoMapper;

namespace AuctionPlatform.Application.Categories.Services;

public class CategoryService : ICategoryService
{
    private readonly IAuctionCategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public CategoryService(IAuctionCategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<List<CategoryTreeNodeDto>> GetCategoryTreeAsync(CancellationToken ct = default)
    {
        var categories = await _categoryRepository.GetAllAsync(ct);

        var dtos = _mapper.Map<List<AuctionCategoryDto>>(categories);

        var roots = dtos
            .Where(c => c.ParentCategoryId is null)
            .Select(c => BuildTreeNode(c, dtos))
            .ToList();

        return roots;
    }

    public async Task<AuctionCategoryDto> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var category = await _categoryRepository.GetBySlugAsync(slug, ct);
        if (category is null)
            throw new KeyNotFoundException($"Category with slug '{slug}' not found.");

        return _mapper.Map<AuctionCategoryDto>(category);
    }

    private static CategoryTreeNodeDto BuildTreeNode(AuctionCategoryDto category, List<AuctionCategoryDto> all)
    {
        return new CategoryTreeNodeDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Children = all
                .Where(c => c.ParentCategoryId == category.Id)
                .Select(c => BuildTreeNode(c, all))
                .ToList()
        };
    }
}
