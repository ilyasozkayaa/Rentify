namespace RentifyApplication.Command.CreateRent;

public sealed record CreateRentCommand(int RenterUserId, int RentableProductId, DateOnly StartDate, DateOnly EndDate) : ICommand<CreateRentResponse>;