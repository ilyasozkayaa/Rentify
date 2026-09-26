namespace RentifyApi.Models;

public sealed record CreateRentRequest(int RentableProductId, DateOnly StartDate, DateOnly EndDate);