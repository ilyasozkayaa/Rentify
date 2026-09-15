using MediatR;
using RentifyApplication.Exceptions;
using RentifyApplication.Exceptions.Enums;
using RentifyApplication.IRepositories;
using RentifyApplication.IServices;

namespace RentifyApplication.Command.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(IUserRepository userRepository, IPasswordHasherService passwordHasher, IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(
            email,
            cancellationToken);

        if (user is null || !_passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            throw new BusinessException("Invalid email or password.", BusinessErrorCode.InvalidCredentials, 401);
        }

        if (!user.IsActive)
        {
            throw new BusinessException("Your account is inactive.", BusinessErrorCode.AccountInactive, 403);
        }

        var accessToken = _jwtTokenService.GenerateToken(user.Id, user.Email, user.IsAdmin);

        return new LoginResponse(user.Id, user.Email, user.FirstName, user.LastName, user.IsAdmin, accessToken);
    }
}
