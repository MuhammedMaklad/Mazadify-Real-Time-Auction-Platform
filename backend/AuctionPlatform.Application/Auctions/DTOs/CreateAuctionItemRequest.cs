namespace AuctionPlatform.Application.Auctions.DTOs;

public class CreateAuctionItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = [];
}
