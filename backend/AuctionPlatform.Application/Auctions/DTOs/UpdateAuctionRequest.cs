namespace AuctionPlatform.Application.Auctions.DTOs;

public class UpdateAuctionRequest
{
    public Guid? CategoryId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? ReservePrice { get; set; }
    public decimal? BidIncrement { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? DeliveryType { get; set; }
    public string? DeliveryNotes { get; set; }
}
