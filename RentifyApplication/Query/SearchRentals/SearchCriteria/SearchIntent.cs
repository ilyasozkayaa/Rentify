using RentifyDomain.Enum;

namespace RentifyApplication.Query.SearchRentals.SearchCriteria;

public sealed record SearchIntent
(
    RentalType RentalType,
    int CityCode,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? SearchText,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? Currency,
    VehicleSearchCriteria? VehicleCriteria,
    PropertySearchCriteria? PropertyCriteria,
    HotelSearchCriteria? HotelCriteria
);
