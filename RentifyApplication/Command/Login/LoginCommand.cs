using MediatR;

namespace RentifyApplication.Command.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;