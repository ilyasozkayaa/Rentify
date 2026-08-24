using FluentValidation;

namespace RentifyApplication.Query.SearchRentals;

public sealed class SearchRentalsQueryValidator : AbstractValidator<SearchRentalsQuery>
{
    public SearchRentalsQueryValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty()
            .WithMessage("Search query cannot be empty.")
            .MaximumLength(500)
            .WithMessage("Search query cannot exceed 500 characters.");
    }
}
