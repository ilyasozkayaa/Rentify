using MediatR;
using RentifyApplication.Exceptions;
using RentifyApplication.Exceptions.Enums;
using RentifyApplication.IRepositories;
using RentifyApplication.IServices;
using RentifyDomain.Entities;

namespace RentifyApplication.Command.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterCommandResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(IUserRepository userRepository, IPasswordHasherService passwordHasher, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<RegisterCommandResponse> Handle(RegisterCommand command, CancellationToken cancellationToken = default)
    {
        var email = command.Email.Trim().ToLowerInvariant();

        var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (existingUser is not null)
        {
            throw new BusinessException("An account with this email already exists.", BusinessErrorCode.EmailAlreadyExists, 409);
        }

        var user = new User
        {
            Email = email,
            PasswordHash = _passwordHasher.Hash(command.Password),
            FirstName = command.FirstName.Trim(),
            LastName = command.LastName.Trim(),
            IsAdmin = false,
            IsActive = true
        };

        await _userRepository.AddAsync(user, cancellationToken);

        return new RegisterCommandResponse(user.Email, user.FirstName, user.LastName);
    }
}
