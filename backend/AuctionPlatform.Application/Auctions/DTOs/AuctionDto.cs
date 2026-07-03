using AuctionPlatform.Application.Categories.DTOs;

namespace AuctionPlatform.Application.Auctions.DTOs;

public class AuctionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public decimal ReservePrice { get; set; }
    public decimal CurrentHighestBid { get; set; }
    public decimal BidIncrement { get; set; }
    public string Status { get; set; } = string.Empty;
    public string DeliveryType { get; set; } = string.Empty;
    public string? DeliveryNotes { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public AuctionCategoryDto Category { get; set; } = null!;
    public UserBriefDto Seller { get; set; } = null!;
    public List<AuctionItemDto> Items { get; set; } = [];
}
