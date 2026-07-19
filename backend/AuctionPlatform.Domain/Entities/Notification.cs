using AuctionPlatform.Domain.ValueTypes;

namespace AuctionPlatform.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Payload { get; set; }   // JSON â€” e.g. { "auctionId": "...", "bidAmount": 150.00 }

    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
