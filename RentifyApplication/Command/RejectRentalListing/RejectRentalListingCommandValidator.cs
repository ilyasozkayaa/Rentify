using FluentValidation;

namespace RentifyApplication.Command.RejectRentalListing;

public sealed class RejectRentalListingCommandValidator : AbstractValidator<RejectRentalListingCommand>
{
    public RejectRentalListingCommandValidator()
    {
        RuleFor(command => command.RentalProductId).GreaterThan(0);
        RuleFor(command => command.Reason)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("A rejection reason is required.")
            .Must(reason => !string.IsNullOrWhiteSpace(reason)).WithMessage("A rejection reason is required.")
            .MaximumLength(1000).WithMessage("A rejection reason cannot exceed 1000 characters.");
    }
}
