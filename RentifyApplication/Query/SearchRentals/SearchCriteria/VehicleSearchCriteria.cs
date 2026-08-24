namespace RentifyApplication.Query.SearchRentals.SearchCriteria;

public sealed record VehicleSearchCriteria
(
    string? Brand,
    string? Model,
    int? ModelYear,
    string? Transmission,
    string? FuelType,
    int? Seats
);
