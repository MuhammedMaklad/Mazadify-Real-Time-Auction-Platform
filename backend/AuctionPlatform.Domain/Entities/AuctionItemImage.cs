namespace AuctionPlatform.Domain.Entities;

public class AuctionItemImage
{
    public Guid Id { get; set; }
    public Guid AuctionItemId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }

    // Navigation
    public AuctionItem AuctionItem { get; set; } = null!;
}
