using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AuctionPlatform.Application.Hubs;

[Authorize]
public class AuctionHub : Hub
{
    public async Task JoinAuctionGroup(Guid auctionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"auction-{auctionId}");

        await Clients.Caller.SendAsync("JoinedAuction", new
        {
            AuctionId = auctionId,
            ServerTimeUtc = DateTime.UtcNow
        });
    }

    public async Task LeaveAuctionGroup(Guid auctionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"auction-{auctionId}");
        await Clients.Caller.SendAsync("LeftAuction", new { AuctionId = auctionId });
    }

    public override async Task OnConnectedAsync()
    {
        var userId = ResolveUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = ResolveUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    private string? ResolveUserId()
    {
        return Context.UserIdentifier
            ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue("sub");
    }
}
