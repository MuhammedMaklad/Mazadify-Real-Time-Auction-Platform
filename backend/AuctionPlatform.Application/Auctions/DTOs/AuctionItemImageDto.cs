namespace AuctionPlatform.Application.Auctions.DTOs;

public class AuctionItemImageDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}
