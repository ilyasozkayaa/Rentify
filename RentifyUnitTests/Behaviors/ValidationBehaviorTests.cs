using FluentValidation;
using MediatR;
using RentifyApplication.Behaviors;
using RentifyApplication.Exceptions;

namespace RentifyUnitTests.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Should_throw_validation_exception_when_validation_fails()
    {
        // Arrange
        var validator = new TestRequestValidator();

        var behavior = new ValidationBehavior<TestRequest, string>(
            new[] { validator });

        var request = new TestRequest(string.Empty);

        var nextCalled = false;

        Task<string> Next(CancellationToken cancellationToken)
        {
            nextCalled = true;

            return Task.FromResult("success");
        }

        // Act
        var exception = await Assert.ThrowsAsync<ValidationFailedException>(
            () => behavior.Handle(
                request,
                Next,
                CancellationToken.None));

        // Assert
        Assert.False(nextCalled);
        Assert.Contains("Value", exception.Errors.Keys);
    }

    [Fact]
    public async Task Should_call_next_when_validation_succeeds()
    {
        // Arrange
        var validator = new TestRequestValidator();

        var behavior = new ValidationBehavior<TestRequest, string>(
            new[] { validator });

        var request = new TestRequest("valid value");

        var nextCalled = false;

        Task<string> Next(CancellationToken cancellationToken)
        {
            nextCalled = true;

            return Task.FromResult("success");
        }

        // Act
        var result = await behavior.Handle(
            request,
            Next,
            CancellationToken.None);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal("success", result);
    }

    private sealed record TestRequest(string Value) : IRequest<string>;

    private sealed class TestRequestValidator
        : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Value)
                .NotEmpty();
        }
    }
}
