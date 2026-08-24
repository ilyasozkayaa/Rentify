using FluentValidation.TestHelper;
using RentifyApplication.Query.SearchRentals;

namespace RentifyUnitTests.ValidatorTests;

public sealed class SearchRentalsQueryValidatorTests
{
    private readonly SearchRentalsQueryValidator _validator = new();

    [Fact]
    public void Should_have_error_when_query_is_empty()
    {
        var query = new SearchRentalsQuery(string.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Query);
    }

    [Fact]
    public void Should_not_have_error_when_query_is_valid()
    {
        var query = new SearchRentalsQuery(
            "Antalya'da müstakil deniz manzaralı villa");

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.Query);
    }

    [Fact]
    public void Should_have_error_when_query_exceeds_maximum_length()
    {
        var query = new SearchRentalsQuery(
            new string('a', 501));

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Query);
    }
}
