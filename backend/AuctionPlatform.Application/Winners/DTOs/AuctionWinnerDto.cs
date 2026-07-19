using AuctionPlatform.Domain.ValueTypes;
using System;

namespace AuctionPlatform.Application.Winners.DTOs
{
    public class AuctionWinnerDto
    {
        public Guid Id { get; set; }
        public Guid AuctionId { get; set; }
        public Guid WinnerId { get; set; }
        public Guid WinningBidId { get; set; }
        public Guid? PaymentMethodId { get; set; }

        public decimal FinalPrice { get; set; }
        public decimal ShippingCost { get; set; }

        public PaymentStatus PaymentStatus { get; set; }
        public DeliveryStatus DeliveryStatus { get; set; }
        public string? TrackingNumber { get; set; }

        public DateTime AwardedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
