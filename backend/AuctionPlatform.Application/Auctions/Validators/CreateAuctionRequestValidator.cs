using AuctionPlatform.Application.Auctions.DTOs;
using FluentValidation;

namespace AuctionPlatform.Application.Auctions.Validators;

public class CreateAuctionRequestValidator : AbstractValidator<CreateAuctionRequest>
{
    public CreateAuctionRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(5000);

        RuleFor(x => x.StartingPrice)
            .GreaterThan(0);

        RuleFor(x => x.ReservePrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.BidIncrement)
            .GreaterThan(0);

        RuleFor(x => x.StartTime)
            .GreaterThan(DateTime.UtcNow)
            .LessThan(x => x.EndTime);

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime);

        RuleFor(x => x.DeliveryType)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.DeliveryNotes)
            .MaximumLength(1000);

        RuleFor(x => x.Items)
            .NotEmpty();

        RuleForEach(x => x.Items)
            .SetValidator(new CreateAuctionItemRequestValidator());
    }
}

public class CreateAuctionItemRequestValidator : AbstractValidator<CreateAuctionItemRequest>
{
    public CreateAuctionItemRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.Condition)
            .NotEmpty()
            .MaximumLength(30);
    }
}
