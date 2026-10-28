using RentifyDomain.Enum;

namespace RentifyApplication.Query.GetMyRents;

public sealed record MyRentsResponse(int Page, int PageSize, bool HasNextPage, IReadOnlyCollection<MyRentResult> Results);

public sealed record MyRentResult(
    int Id,
    int RentableProductId,
    string ListingTitle,
    string RentalType,
    string City,
    string? District,
    string? CoverImageUrl,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal DailyPrice,
    decimal TotalPrice,
    string Currency,
    RentStatus Status,
    DateTime CreatedAt);
