using AuctionPlatform.Application.Winners.DTOs;
using AuctionPlatform.Domain.ValueTypes;
using FluentValidation;
using System;

namespace AuctionPlatform.Application.Winners.Validators
{
    public class UpdateDeliveryStatusValidator : AbstractValidator<UpdateDeliveryStatusRequest>
    {
        public UpdateDeliveryStatusValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Delivery status must not be empty.")
                .Must(status => Enum.TryParse<DeliveryStatus>(status, true, out _))
                .WithMessage("Invalid delivery status value.");
        }
    }
}
