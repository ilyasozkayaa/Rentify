using FluentValidation.TestHelper;
using RentifyApplication.Query.GetPendingRentalListings;

namespace RentifyUnitTests.ValidatorTests;

public sealed class GetPendingRentalListingsQueryValidatorTests
{
    private readonly GetPendingRentalListingsQueryValidator _validator = new();

    [Fact]
    public void Should_accept_default_pagination()
    {
        var result = _validator.TestValidate(new GetPendingRentalListingsQuery());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_reject_invalid_page_and_page_size()
    {
        var result = _validator.TestValidate(new GetPendingRentalListingsQuery(Page: 0, PageSize: 101));

        result.ShouldHaveValidationErrorFor(query => query.Page);
        result.ShouldHaveValidationErrorFor(query => query.PageSize);
    }
}
