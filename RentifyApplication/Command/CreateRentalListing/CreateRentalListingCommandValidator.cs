using FluentValidation;
using System.Text.Json;
using RentifyDomain.Enum;

namespace RentifyApplication.Command.CreateRentalListing;

public sealed class CreateRentalListingCommandValidator : AbstractValidator<CreateRentalListingCommand>
{
    public CreateRentalListingCommandValidator()
    {
        RuleFor(x => x.OwnerUserId).GreaterThan(0);

        RuleFor(x => x.RentalType)
            .Must(value => Enum.IsDefined(typeof(RentalType), value) && value != (int)RentalType.Unknown).WithMessage("Please provide a supported rental type.");
        RuleFor(x => x.CityCode)
            .Must(value => Enum.IsDefined(typeof(CityCode), value)).WithMessage("Please provide a valid city.");
        RuleFor(x => x.District).MaximumLength(100);

        RuleFor(x => x.Title)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Title is required.").MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");
        RuleFor(x => x.Description).MaximumLength(2000);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.")
            .PrecisionScale(18, 2, ignoreTrailingZeros: true).WithMessage("Price must fit within 18 digits and have no more than 2 decimal places.");
        RuleFor(x => x.Currency)
            .Must(value => Enum.TryParse<Currency>(value, true, out var currency) && Enum.IsDefined(currency)).WithMessage("Please provide a supported currency.");

        RuleFor(x => x.Attributes)
            .Must(attributes => attributes.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined).WithMessage("Attributes are required.");
    }
}
