namespace AuctionPlatform.Application.Notifications.DTOs;

public abstract record BroadcastTarget
{
    private BroadcastTarget() { }

    public sealed record All : BroadcastTarget;
    public sealed record AuctionGroup(Guid AuctionId) : BroadcastTarget;
    public sealed record User(Guid UserId) : BroadcastTarget;
}
