namespace AuctionPlatform.Application.Common.Exceptions;

/// <summary>
/// Thrown when a bid placement fails due to concurrent updates.
/// Maps to HTTP 409 Conflict in the controller.
/// </summary>
public class BidConcurrencyException : InvalidOperationException
{
    public BidConcurrencyException()
        : base("Bid placement failed due to concurrent bidding activity. Please refresh the auction and try again.")
    {
    }

    public BidConcurrencyException(string message)
        : base(message)
    {
    }

    public BidConcurrencyException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
