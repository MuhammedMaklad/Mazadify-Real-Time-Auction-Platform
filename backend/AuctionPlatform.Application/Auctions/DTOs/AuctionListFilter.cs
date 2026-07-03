namespace AuctionPlatform.Application.Auctions.DTOs;

public class AuctionListFilter
{
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? SellerId { get; set; }
    public string? Status { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
