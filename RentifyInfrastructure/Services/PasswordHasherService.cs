using Microsoft.AspNetCore.Identity;
using RentifyApplication.IServices;

namespace RentifyInfrastructure.Services;

public sealed class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password)
    {
        return _hasher.HashPassword(null!, password);
    }

    public bool Verify(string password, string passwordHash)
    {
        return _hasher.VerifyHashedPassword(null!, passwordHash, password) == PasswordVerificationResult.Success;
    }
}
