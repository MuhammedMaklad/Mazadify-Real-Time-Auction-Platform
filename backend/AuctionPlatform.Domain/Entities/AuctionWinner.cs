using AuctionPlatform.Domain.ValueTypes;

namespace AuctionPlatform.Domain.Entities;

public class AuctionWinner
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Guid WinnerId { get; set; }
    public Guid WinningBidId { get; set; }
    public Guid? PaymentMethodId { get; set; }

    public decimal FinalPrice { get; set; }
    public decimal ShippingCost { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.Pending;
    public string? TrackingNumber { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public DateTime AwardedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }

    // Navigation
    public Auction Auction { get; set; } = null!;
    public User Winner { get; set; } = null!;
    public Bid WinningBid { get; set; } = null!;
    public PaymentMethod? PaymentMethod { get; set; }
}
