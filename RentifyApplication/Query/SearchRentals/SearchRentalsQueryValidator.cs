using FluentValidation;

namespace RentifyApplication.Query.SearchRentals;

public sealed class SearchRentalsQueryValidator : AbstractValidator<SearchRentalsQuery>
{
    public SearchRentalsQueryValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty()
            .WithMessage("Please enter what you are looking for.")
            .Must(query => query.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 3)
            .WithMessage("Please provide at least 3 words to describe your search.")
            .Must(query => query.Count(char.IsLetterOrDigit) >= 15)
            .WithMessage("Please provide a little more detail about your search.")
            .MaximumLength(500)
            .WithMessage("Your search cannot exceed 500 characters.");
    }
}
