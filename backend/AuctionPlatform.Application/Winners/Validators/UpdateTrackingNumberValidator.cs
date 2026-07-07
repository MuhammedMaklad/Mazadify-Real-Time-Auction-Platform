using AuctionPlatform.Application.Winners.DTOs;
using FluentValidation;

namespace AuctionPlatform.Application.Winners.Validators
{
    public class UpdateTrackingNumberValidator : AbstractValidator<UpdateTrackingNumberRequest>
    {
        public UpdateTrackingNumberValidator()
        {
            RuleFor(x => x.TrackingNumber)
                .NotEmpty().WithMessage("Tracking number must not be empty.")
                .MaximumLength(100).WithMessage("Tracking number must not exceed 100 characters.");
        }
    }
}
