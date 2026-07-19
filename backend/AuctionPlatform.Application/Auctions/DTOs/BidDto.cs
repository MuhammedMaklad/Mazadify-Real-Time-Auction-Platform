namespace AuctionPlatform.Application.Auctions.DTOs;

public class BidDto
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Guid BidderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime PlacedAt { get; set; }
    public bool IsAutoBid { get; set; }
    public UserBriefDto Bidder { get; set; } = null!;
}
