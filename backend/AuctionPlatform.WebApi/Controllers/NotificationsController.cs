using System.Security.Claims;
using AuctionPlatform.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionPlatform.WebApi.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _notificationService.GetUserNotificationsAsync(
            userId, unreadOnly, page, pageSize, ct);

        return Ok(result);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        await _notificationService.MarkAsReadAsync(id, userId, ct);
        return NoContent();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        await _notificationService.MarkAllAsReadAsync(userId, ct);
        return NoContent();
    }

    private Guid ResolveUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? User.FindFirstValue(ClaimTypes.Name);

        return value is not null && Guid.TryParse(value, out var userId)
            ? userId
            : Guid.Empty;
    }
}
