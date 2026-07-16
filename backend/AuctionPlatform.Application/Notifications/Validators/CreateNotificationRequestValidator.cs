using AuctionPlatform.Application.Notifications.DTOs;
using FluentValidation;

namespace AuctionPlatform.Application.Notifications.Validators;

public class CreateNotificationRequestValidator : AbstractValidator<CreateNotificationRequest>
{
    public CreateNotificationRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Message)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.Payload)
            .MaximumLength(5000)
            .When(x => x.Payload is not null);
    }
}
