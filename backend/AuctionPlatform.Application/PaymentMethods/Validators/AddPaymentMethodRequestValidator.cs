using AuctionPlatform.Application.PaymentMethods.DTOs;
using AuctionPlatform.Domain.ValueTypes;
using FluentValidation;

namespace AuctionPlatform.Application.PaymentMethods.Validators;

public class AddPaymentMethodRequestValidator : AbstractValidator<AddPaymentMethodRequest>
{
    private static readonly string[] AllowedTypes =
        Enum.GetNames<PaymentMethodType>();

    public AddPaymentMethodRequestValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Payment method type is required.")
            .Must(t => AllowedTypes.Contains(t))
            .WithMessage($"Type must be one of: {string.Join(", ", AllowedTypes)}.");

        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required.")
            .MaximumLength(100).WithMessage("Provider must not exceed 100 characters.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Payment token is required.")
            .MaximumLength(500).WithMessage("Token must not exceed 500 characters.");

        RuleFor(x => x.LastFour)
            .Matches("^[0-9]{4}$")
            .When(x => x.LastFour is not null)
            .WithMessage("LastFour must be exactly 4 digits.");

        RuleFor(x => x.ExpiryMonth)
            .Matches("^(0[1-9]|1[0-2])$")
            .When(x => x.ExpiryMonth is not null)
            .WithMessage("ExpiryMonth must be MM format (01–12).");

        RuleFor(x => x.ExpiryYear)
            .Matches("^[0-9]{4}$")
            .When(x => x.ExpiryYear is not null)
            .WithMessage("ExpiryYear must be a 4-digit year.");
    }
}
