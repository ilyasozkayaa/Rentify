using FluentValidation.TestHelper;
using System.Text.Json;
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

    [Fact]
    public void Should_reject_missing_attributes()
    {
        var result = _validator.TestValidate(ValidCommand() with { Attributes = default });

        result.ShouldHaveValidationErrorFor(x => x.Attributes);
    }

    [Fact]
    public void Should_reject_json_null_attributes()
    {
        using var attributes = JsonDocument.Parse("null");
        var result = _validator.TestValidate(ValidCommand() with { Attributes = attributes.RootElement });

        result.ShouldHaveValidationErrorFor(x => x.Attributes);
    }

    private static CreateRentalListingCommand ValidCommand() =>
        new(1, 2, 7, null, "Apartment", null, 100, "TRY", JsonDocument.Parse("{}").RootElement);
}
