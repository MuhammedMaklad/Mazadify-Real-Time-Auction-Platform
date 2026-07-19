using AuctionPlatform.Application.AutoBids.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuctionPlatform.Application.AutoBids.Validators
{
    public class UpdateAutoBidValidator : AbstractValidator<UpdateAutoBidRequest>
    {
        public UpdateAutoBidValidator()
        {
            RuleFor(x => x.MaxAmount)
                .GreaterThan(0)
                .PrecisionScale(18, 2, false)
                .WithMessage("Maximum bid amount must be greater than zero.");
        }
    }
}
