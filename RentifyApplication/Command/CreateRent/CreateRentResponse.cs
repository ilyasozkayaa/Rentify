using RentifyDomain.Enum;

namespace RentifyApplication.Command.CreateRent;

public sealed record CreateRentResponse(int RentableProductId, DateOnly StartDate, DateOnly EndDate, decimal TotalPrice, string Currency, RentStatus Status);