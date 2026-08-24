using RentifyDomain.Enum;

namespace RentifyApplication.Query.SearchRentals.SearchCriteria;

public sealed record SearchIntent
(
    RentalType RentalType,
    string? Location,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? SearchText,
    decimal? MinPrice,
    decimal? MaxPrice,
    VehicleSearchCriteria? VehicleCriteria,
    PropertySearchCriteria? PropertyCriteria,
    HotelSearchCriteria? HotelCriteria
);
