using RentifyDomain.Enum;

namespace RentifyApplication.Query.SearchRentals;

public sealed record RentalSearchResult
(
    Guid Id,
    RentalType Type,
    string Name,
    string? Location,
    decimal Price,
    string? Description
);
