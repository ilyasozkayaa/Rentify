using MediatR;

namespace RentifyApplication.Command.Register;

public sealed record RegisterCommand(string Email, string Password, string FirstName, string LastName) : IRequest<RegisterCommandResponse>;
