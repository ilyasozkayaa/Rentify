using FluentValidation;

namespace RentifyApplication.Command.CreateRent;

public sealed class CreateRentCommandValidator : AbstractValidator<CreateRentCommand>
{
    public CreateRentCommandValidator()
    {
        RuleFor(command => command.RenterUserId).GreaterThan(0);

        RuleFor(command => command.RentableProductId).GreaterThan(0);

        RuleFor(command => command.StartDate)
            .Must(startDate => startDate >= DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("Start date cannot be in the past.");

        RuleFor(command => command.EndDate)
            .Must((command, endDate) => endDate > command.StartDate).WithMessage("End date must be after start date.");
    }
}