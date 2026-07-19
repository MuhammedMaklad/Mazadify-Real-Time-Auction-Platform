namespace AuctionPlatform.Application.PaymentMethods.DTOs;

public class AddPaymentMethodRequest
{
    public string Type { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string? LastFour { get; set; }
    public string? ExpiryMonth { get; set; }
    public string? ExpiryYear { get; set; }
    public bool IsDefault { get; set; }
}
