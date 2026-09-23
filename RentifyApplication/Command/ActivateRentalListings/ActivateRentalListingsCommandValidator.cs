using FluentValidation;

namespace RentifyApplication.Command.ActivateRentalListings;

public sealed class ActivateRentalListingsCommandValidator : AbstractValidator<ActivateRentalListingsCommand>
{
    public ActivateRentalListingsCommandValidator()
    {
        RuleFor(command => command.RentalProductIds)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("At least one rental product ID is required.")
            .NotEmpty().WithMessage("At least one rental product ID is required.")
            .Must(ids => ids.Count <= 100).WithMessage("No more than 100 rental products can be activated at once.")
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Rental product IDs must be unique.");

        RuleForEach(command => command.RentalProductIds)
            .GreaterThan(0).WithMessage("Rental product IDs must be greater than zero.");
    }
}
