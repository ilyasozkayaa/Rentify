namespace RentifyApplication.Command.Register;

public sealed record RegisterCommandResponse(int Id, string Email, string FirstName, string LastName);