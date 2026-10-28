namespace RentifyApplication.Query.GetMyRents;

public sealed record MyRentRecord(
    int Id,
    int RentableProductId,
    string ListingTitle,
    int RentalType,
    int CityCode,
    string? District,
    string? CoverImageStorageKey,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal DailyPrice,
    decimal TotalPrice,
    string Currency,
    int Status,
    DateTime CreatedAt);
