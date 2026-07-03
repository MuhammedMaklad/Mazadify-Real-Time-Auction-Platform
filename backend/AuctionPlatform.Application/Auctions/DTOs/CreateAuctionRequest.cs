namespace AuctionPlatform.Application.Auctions.DTOs;

public class CreateAuctionRequest
{
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public decimal ReservePrice { get; set; }
    public decimal BidIncrement { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string DeliveryType { get; set; } = string.Empty;
    public string? DeliveryNotes { get; set; }
    public List<CreateAuctionItemRequest> Items { get; set; } = [];
}
