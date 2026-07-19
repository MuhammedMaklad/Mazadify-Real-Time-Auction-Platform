namespace AuctionPlatform.Application.PaymentMethods.DTOs;

public class PaymentMethodDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? LastFour { get; set; }
    public string? ExpiryMonth { get; set; }
    public string? ExpiryYear { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
