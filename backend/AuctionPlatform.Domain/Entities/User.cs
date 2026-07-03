using Microsoft.AspNetCore.Identity;

namespace AuctionPlatform.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Auction> CreatedAuctions { get; set; } = new List<Auction>();
    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    public ICollection<AutoBid> AutoBids { get; set; } = new List<AutoBid>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
    public AuctionWinner? WonAuction { get; set; }
}
