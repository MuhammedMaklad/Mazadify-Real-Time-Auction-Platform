namespace AuctionPlatform.Application.Auctions.DTOs;

public class AuctionSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public decimal ReservePrice { get; set; }
    public decimal CurrentHighestBid { get; set; }
    public decimal BidIncrement { get; set; }
    public string Status { get; set; } = string.Empty;
    public string DeliveryType { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string SellerUsername { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public string? PrimaryImageUrl { get; set; }
}
