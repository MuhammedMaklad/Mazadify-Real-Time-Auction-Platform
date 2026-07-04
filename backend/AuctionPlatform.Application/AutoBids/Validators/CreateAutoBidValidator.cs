using AuctionPlatform.Application.AutoBids.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuctionPlatform.Application.AutoBids.Validators
{
    public class CreateAutoBidValidator : AbstractValidator<CreateAutoBidRequest>
    {
        public CreateAutoBidValidator()
        {
            RuleFor(x => x.MaxAmount)
                .GreaterThan(0)
                .WithMessage("Maximum bid amount must be greater than zero.");
        }
    }
}
