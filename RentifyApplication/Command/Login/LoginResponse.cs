namespace RentifyApplication.Command.Login;

public sealed record LoginResponse(int Id, string Email, string FirstName, string LastName, bool IsAdmin, string AccessToken);
