using AuctionPlatform.Application.Auctions.DTOs;
using FluentValidation;

namespace AuctionPlatform.Application.Auctions.Validators;

public class PlaceBidRequestValidator : AbstractValidator<PlaceBidRequest>
{
    public PlaceBidRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Bid amount must be greater than 0");

        RuleFor(x => x.IdempotencyKey)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.IdempotencyKey))
            .WithMessage("Idempotency key cannot exceed 100 characters");
    }
}
