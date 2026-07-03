namespace AuctionPlatform.Domain.ValueTypes;

public enum AuctionStatus
{
    Draft,
    Scheduled,
    Live,
    Ended,
    Cancelled,
    ReserveNotMet
}

public enum DeliveryType
{
    Pickup,
    Shipping,
    Digital
}

public enum DeliveryStatus
{
    Pending,
    Shipped,
    Delivered,
    PickedUp,
    Cancelled
}

public enum BidStatus
{
    Accepted,
    Rejected,
    Outbid,
    Winning,
    Won,
    Lost
}

public enum PaymentStatus
{
    Pending,
    Paid,
    Failed,
    Refunded
}

public enum PaymentMethodType
{
    CreditCard,
    DebitCard,
    BankTransfer,
    DigitalWallet
}

public enum NotificationType
{
    OutbidAlert,
    AuctionStartingSoon,
    AuctionEnded,
    YouWon,
    PaymentRequired,
    BidAccepted,
    BidRejected
}
