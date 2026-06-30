namespace AuctionPlatform.Domain.Entities;

public class AuctionItem
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;   // New, Used, Refurbished

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Auction Auction { get; set; } = null!;
    public ICollection<AuctionItemImage> Images { get; set; } = new List<AuctionItemImage>();
}
