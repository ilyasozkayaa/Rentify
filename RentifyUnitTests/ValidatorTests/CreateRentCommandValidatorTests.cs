using FluentValidation.TestHelper;
using RentifyApplication.Command.CreateRent;

namespace RentifyUnitTests.ValidatorTests;

public sealed class CreateRentCommandValidatorTests
{
    private readonly CreateRentCommandValidator _validator = new();

    [Fact]
    public void Should_accept_a_valid_future_date_range()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);

        var result = _validator.TestValidate(new CreateRentCommand(1, 2, startDate, startDate.AddDays(1)));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_reject_invalid_ids_and_date_ranges()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var result = _validator.TestValidate(new CreateRentCommand(0, 0, today.AddDays(-1), today.AddDays(-1)));

        result.ShouldHaveValidationErrorFor(command => command.RenterUserId);
        result.ShouldHaveValidationErrorFor(command => command.RentableProductId);
        result.ShouldHaveValidationErrorFor(command => command.StartDate);
        result.ShouldHaveValidationErrorFor(command => command.EndDate);
    }
}