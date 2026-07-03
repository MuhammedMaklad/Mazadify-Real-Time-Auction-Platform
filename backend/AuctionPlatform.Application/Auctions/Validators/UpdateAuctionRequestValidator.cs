using AuctionPlatform.Application.Auctions.DTOs;
using FluentValidation;

namespace AuctionPlatform.Application.Auctions.Validators;

public class UpdateAuctionRequestValidator : AbstractValidator<UpdateAuctionRequest>
{
    public UpdateAuctionRequestValidator()
    {
        RuleFor(x => x).Must(x => AtLeastOnePropertySet(x))
            .WithMessage("At least one field must be provided for update.");

        When(x => x.CategoryId.HasValue, () =>
        {
            RuleFor(x => x.CategoryId!.Value).NotEmpty();
        });

        When(x => x.Title != null, () =>
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        });

        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description).NotEmpty().MaximumLength(5000);
        });

        When(x => x.ReservePrice.HasValue, () =>
        {
            RuleFor(x => x.ReservePrice!.Value).GreaterThanOrEqualTo(0);
        });

        When(x => x.BidIncrement.HasValue, () =>
        {
            RuleFor(x => x.BidIncrement!.Value).GreaterThan(0);
        });

        When(x => x.StartTime.HasValue && x.EndTime.HasValue, () =>
        {
            RuleFor(x => x.StartTime!.Value).LessThan(x => x.EndTime!.Value);
            RuleFor(x => x.EndTime!.Value).GreaterThan(x => x.StartTime!.Value);
        });

        When(x => x.DeliveryType != null, () =>
        {
            RuleFor(x => x.DeliveryType).NotEmpty().MaximumLength(20);
        });

        When(x => x.DeliveryNotes != null, () =>
        {
            RuleFor(x => x.DeliveryNotes).MaximumLength(1000);
        });
    }

    private static bool AtLeastOnePropertySet(UpdateAuctionRequest request)
    {
        return request.CategoryId.HasValue ||
               request.Title != null ||
               request.Description != null ||
               request.ReservePrice.HasValue ||
               request.BidIncrement.HasValue ||
               request.StartTime.HasValue ||
               request.EndTime.HasValue ||
               request.DeliveryType != null ||
               request.DeliveryNotes != null;
    }
}
