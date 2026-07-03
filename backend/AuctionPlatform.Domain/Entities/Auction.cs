using AuctionPlatform.Domain.ValueTypes;

namespace AuctionPlatform.Domain.Entities;

public class Auction
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public Guid CategoryId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public decimal StartingPrice { get; set; }
    public decimal ReservePrice { get; set; }
    public decimal CurrentHighestBid { get; set; }
    public decimal BidIncrement { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AuctionStatus Status { get; set; } = AuctionStatus.Draft;

    public DeliveryType DeliveryType { get; set; }
    public string? DeliveryNotes { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public User Seller { get; set; } = null!;
    public AuctionCategory Category { get; set; } = null!;
    public ICollection<AuctionItem> Items { get; set; } = new List<AuctionItem>();
    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    public ICollection<AutoBid> AutoBids { get; set; } = new List<AutoBid>();
    public AuctionWinner? Winner { get; set; }
}
