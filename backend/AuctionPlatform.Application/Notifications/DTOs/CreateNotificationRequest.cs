using AuctionPlatform.Domain.ValueTypes;

namespace AuctionPlatform.Application.Notifications.DTOs;

public class CreateNotificationRequest
{
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Payload { get; set; }
    public Guid? RelatedAuctionId { get; set; }
}
