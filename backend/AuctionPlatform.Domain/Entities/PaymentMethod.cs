using AuctionPlatform.Domain.ValueTypes;

namespace AuctionPlatform.Domain.Entities;

public class PaymentMethod
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public PaymentMethodType Type { get; set; }
    public string Provider { get; set; } = string.Empty;     // e.g. "Stripe", "PayPal"
    public string Token { get; set; } = string.Empty;        // gateway token — never raw card data
    public string? LastFour { get; set; }
    public string? ExpiryMonth { get; set; }
    public string? ExpiryYear { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<AuctionWinner> SettledPayments { get; set; } = new List<AuctionWinner>();
}
