namespace AuctionPlatform.Application.Categories.DTOs;

public class CategoryTreeNodeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public List<CategoryTreeNodeDto> Children { get; set; } = [];
}
