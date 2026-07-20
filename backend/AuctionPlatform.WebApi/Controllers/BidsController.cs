using AuctionPlatform.Application.Auctions.DTOs;
using AuctionPlatform.Application.Auctions.Interfaces;
using AuctionPlatform.Application.Auctions.Validators;
using AuctionPlatform.Application.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace AuctionPlatform.WebApi.Controllers;

/// <summary>
/// Manages bid placement, history, and highest bid queries.
/// </summary>
[ApiController]
[Route("api/auctions/{auctionId}/bids")]
//[Authorize]
public class BidsController : ControllerBase
{
    private readonly IBidService _bidService;
    private readonly PlaceBidRequestValidator _bidValidator;

    public BidsController(
        IBidService bidService,
        PlaceBidRequestValidator bidValidator)
    {
        _bidService = bidService;
        _bidValidator = bidValidator;
    }

    /// <summary>
    /// Places a bid on an auction.
    /// </summary>
    /// <param name="auctionId">The auction ID</param>
    /// <param name="request">The bid request (amount, optional idempotency key)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The placed bid DTO</returns>
    /// <response code="201">Bid accepted</response>
    /// <response code="400">Validation error or business rule violation</response>
    /// <response code="404">Auction not found</response>
    /// <response code="409">Concurrency conflict; another bid was placed simultaneously</response>
    /// <response code="429">Rate limit exceeded</response>
    [HttpPost]
    [EnableRateLimiting("per_user_bid_limit")]
    public async Task<IActionResult> PlaceBid(
        Guid auctionId,
        [FromBody] PlaceBidRequest request,
        CancellationToken ct)
    {
        // Validate request
        var validation = await _bidValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        try
        {
            // Extract bidderId from authenticated user (JWT claim: NameIdentifier)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var bidderId))
                return Unauthorized(new { message = "User identity claim not found or invalid." });

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var result = await _bidService.PlaceBidAsync(auctionId, bidderId, request, ipAddress, ct: ct);
            return CreatedAtAction(nameof(GetHighestBid), new { auctionId }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (BidConcurrencyException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves paginated bid history for an auction.
    /// </summary>
    /// <param name="auctionId">The auction ID</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Items per page (1-100)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged bid history</returns>
    /// <response code="200">Success</response>
    [HttpGet]
    public async Task<IActionResult> GetHistory(
        Guid auctionId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _bidService.GetHistoryAsync(auctionId, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the highest bid for an auction.
    /// </summary>
    /// <param name="auctionId">The auction ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The highest bid DTO, or null if no bids exist</returns>
    /// <response code="200">Success</response>
    [HttpGet("highest")]
    public async Task<IActionResult> GetHighestBid(
        Guid auctionId,
        CancellationToken ct = default)
    {
        var result = await _bidService.GetHighestAsync(auctionId, ct);
        if (result is null)
            return Ok(null);

        return Ok(result);
    }
}
