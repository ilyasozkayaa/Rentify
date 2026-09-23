using FluentValidation.TestHelper;
using RentifyApplication.Command.CreateRentalListing;

namespace RentifyUnitTests.ValidatorTests;

public sealed class CreateRentalListingCommandValidatorTests
{
    private readonly CreateRentalListingCommandValidator _validator = new();

    [Fact]
    public void Should_accept_a_valid_request()
    {
        var result = _validator.TestValidate(ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_reject_missing_required_values()
    {
        var command = ValidCommand() with { Title = "", Currency = "" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title);
        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Fact]
    public void Should_reject_invalid_type_city_and_price()
    {
        var command = ValidCommand() with { RentalType = 99, CityCode = 0, Price = 0 };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.RentalType);
        result.ShouldHaveValidationErrorFor(x => x.CityCode);
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    private static CreateRentalListingCommand ValidCommand() =>
        new(1, 2, 7, null, "Apartment", null, 100, "TRY", null);
}
