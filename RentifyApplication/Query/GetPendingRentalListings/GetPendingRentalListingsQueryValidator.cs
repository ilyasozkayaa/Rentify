using FluentValidation;

namespace RentifyApplication.Query.GetPendingRentalListings;

public sealed class GetPendingRentalListingsQueryValidator : AbstractValidator<GetPendingRentalListingsQuery>
{
    public GetPendingRentalListingsQueryValidator()
    {
        RuleFor(query => query.Page)
            .InclusiveBetween(1, 100_000).WithMessage("Page must be between 1 and 100000.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}
