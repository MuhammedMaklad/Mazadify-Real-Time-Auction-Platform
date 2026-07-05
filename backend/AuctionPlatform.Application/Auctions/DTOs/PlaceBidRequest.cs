namespace AuctionPlatform.Application.Auctions.DTOs;

public class PlaceBidRequest
{
    public decimal Amount { get; set; }
    public string? IdempotencyKey { get; set; }
}
