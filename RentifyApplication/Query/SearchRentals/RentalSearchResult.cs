using RentifyDomain.Enum;

namespace RentifyApplication.Query.SearchRentals;

public sealed record RentalSearchResult
(
    int Id,
    string RentalType,
    string Title,
    string? City,
    decimal Price,
    string Currency,
    string? Description
);
