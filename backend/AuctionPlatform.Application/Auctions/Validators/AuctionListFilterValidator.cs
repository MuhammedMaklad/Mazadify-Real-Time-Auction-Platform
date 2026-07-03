using AuctionPlatform.Application.Auctions.DTOs;
using FluentValidation;

namespace AuctionPlatform.Application.Auctions.Validators;

public class AuctionListFilterValidator : AbstractValidator<AuctionListFilter>
{
    private static readonly string[] AllowedSortFields =
        ["startTime", "endTime", "currentHighestBid", "title"];

    public AuctionListFilterValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        When(x => x.SortBy != null, () =>
        {
            RuleFor(x => x.SortBy)
                .Must(x => AllowedSortFields.Contains(x))
                .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortFields)}");
        });
    }
}
