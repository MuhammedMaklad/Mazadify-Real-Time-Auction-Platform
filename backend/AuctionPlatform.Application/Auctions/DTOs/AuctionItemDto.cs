namespace AuctionPlatform.Application.Auctions.DTOs;

public class AuctionItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public List<AuctionItemImageDto> Images { get; set; } = [];
}
