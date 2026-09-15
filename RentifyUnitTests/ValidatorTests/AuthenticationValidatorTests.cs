using FluentValidation.TestHelper;
using RentifyApplication.Command.Login;
using RentifyApplication.Command.Register;

namespace RentifyUnitTests.ValidatorTests;

public sealed class AuthenticationValidatorTests
{
    [Fact]
    public void Register_should_accept_a_password_that_login_also_accepts()
    {
        var command = new RegisterCommand(
            "user@example.com",
            new string('p', 25),
            "Ada",
            "Lovelace");

        var result = new RegisterCommandValidator().TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Login_should_accept_the_maximum_register_password_length()
    {
        var command = new LoginCommand(
            "user@example.com",
            new string('p', 25));

        var result = new LoginCommandValidator().TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Register_should_reject_a_last_name_longer_than_the_database_limit()
    {
        var command = new RegisterCommand(
            "user@example.com",
            "password123",
            "Ada",
            new string('L', 51));

        var result = new RegisterCommandValidator().TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }
}
