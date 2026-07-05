using AuctionPlatform.Application.Auctions.DTOs;
using Microsoft.Extensions.Logging;

namespace AuctionPlatform.Application.Auctions.Services;

/// <summary>
/// Audit service for tracking bid placements.
/// Logs structured entries with context for compliance and troubleshooting.
/// </summary>
public interface IBidAuditService
{
    /// <summary>
    /// Logs a bid placement event.
    /// </summary>
    Task LogBidPlacedAsync(
        Guid auctionId,
        Guid bidderId,
        decimal amount,
        string? ipAddress = null,
        string? idempotencyKey = null,
        CancellationToken ct = default);

    /// <summary>
    /// Logs a bid outbid event.
    /// </summary>
    Task LogBidOutbidAsync(
        Guid auctionId,
        Guid previousBidderId,
        decimal newHighestAmount,
        string? ipAddress = null,
        CancellationToken ct = default);

    /// <summary>
    /// Logs a bid validation failure.
    /// </summary>
    Task LogBidRejectedAsync(
        Guid auctionId,
        Guid bidderId,
        string reason,
        string? ipAddress = null,
        CancellationToken ct = default);
}

public class BidAuditService : IBidAuditService
{
    private readonly ILogger<BidAuditService> _logger;

    public BidAuditService(ILogger<BidAuditService> logger)
    {
        _logger = logger;
    }

    public async Task LogBidPlacedAsync(
        Guid auctionId,
        Guid bidderId,
        decimal amount,
        string? ipAddress = null,
        string? idempotencyKey = null,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "BID_PLACED: AuctionId={AuctionId}, BidderId={BidderId}, Amount={Amount:C}, IpAddress={IpAddress}, IdempotencyKey={IdempotencyKey}",
            auctionId, bidderId, amount, ipAddress ?? "unknown", idempotencyKey ?? "none");

        await Task.CompletedTask;
    }

    public async Task LogBidOutbidAsync(
        Guid auctionId,
        Guid previousBidderId,
        decimal newHighestAmount,
        string? ipAddress = null,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "BID_OUTBID: AuctionId={AuctionId}, PreviousBidderId={PreviousBidderId}, NewHighestAmount={NewHighestAmount:C}, IpAddress={IpAddress}",
            auctionId, previousBidderId, newHighestAmount, ipAddress ?? "unknown");

        await Task.CompletedTask;
    }

    public async Task LogBidRejectedAsync(
        Guid auctionId,
        Guid bidderId,
        string reason,
        string? ipAddress = null,
        CancellationToken ct = default)
    {
        _logger.LogWarning(
            "BID_REJECTED: AuctionId={AuctionId}, BidderId={BidderId}, Reason={Reason}, IpAddress={IpAddress}",
            auctionId, bidderId, reason, ipAddress ?? "unknown");

        await Task.CompletedTask;
    }
}
