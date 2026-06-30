namespace AuctionPlatform.Domain.Entities;

public class AutoBid
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Guid BidderId { get; set; }

    public decimal MaxAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Auction Auction { get; set; } = null!;
    public User Bidder { get; set; } = null!;
}
