namespace RentifyApplication.IServices;

public interface IJwtTokenService
{
    string GenerateToken(int userId, string email, bool isAdmin);
}
