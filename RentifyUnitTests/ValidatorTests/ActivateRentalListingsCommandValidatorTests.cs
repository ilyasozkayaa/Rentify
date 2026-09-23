using FluentValidation.TestHelper;
using RentifyApplication.Command.ActivateRentalListings;

namespace RentifyUnitTests.ValidatorTests;

public sealed class ActivateRentalListingsCommandValidatorTests
{
    private readonly ActivateRentalListingsCommandValidator _validator = new();

    [Fact]
    public void Should_accept_unique_positive_ids_within_batch_limit()
    {
        var result = _validator.TestValidate(new ActivateRentalListingsCommand([1, 2, 3]));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_reject_empty_duplicate_or_non_positive_ids()
    {
        _validator.TestValidate(new ActivateRentalListingsCommand([]))
            .ShouldHaveValidationErrorFor(command => command.RentalProductIds);
        _validator.TestValidate(new ActivateRentalListingsCommand([1, 1]))
            .ShouldHaveValidationErrorFor(command => command.RentalProductIds);
        var nonPositiveIdResult = _validator.TestValidate(new ActivateRentalListingsCommand([0]));
        Assert.Contains(nonPositiveIdResult.Errors, error => error.PropertyName.StartsWith(nameof(ActivateRentalListingsCommand.RentalProductIds)));
    }

    [Fact]
    public void Should_reject_batches_over_one_hundred_ids()
    {
        var result = _validator.TestValidate(new ActivateRentalListingsCommand(Enumerable.Range(1, 101).ToArray()));

        result.ShouldHaveValidationErrorFor(command => command.RentalProductIds);
    }
}
