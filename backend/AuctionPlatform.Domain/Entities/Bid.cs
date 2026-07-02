using System.ComponentModel.DataAnnotations;
using AuctionPlatform.Domain.Enums;

namespace AuctionPlatform.Domain.Entities;

public class Bid
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Guid BidderId { get; set; }

    public decimal Amount { get; set; }
    public BidStatus Status { get; set; }
    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;

    public string? IpAddress { get; set; }
    public bool IsAutoBid { get; set; }

    // Optimistic concurrency token — EF Core throws DbUpdateConcurrencyException
    // on conflicting writes from concurrent bidders.
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation
    public Auction Auction { get; set; } = null!;
    public User Bidder { get; set; } = null!;
    public AuctionWinner? AuctionWinner { get; set; }
}
