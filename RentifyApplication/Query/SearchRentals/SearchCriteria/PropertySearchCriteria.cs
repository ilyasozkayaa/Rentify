namespace RentifyApplication.Query.SearchRentals.SearchCriteria;

public sealed record PropertySearchCriteria
(
    string? PropertyType,
    bool? SeaView,
    bool? Detached,
    int? Bedrooms,
    bool? Pool
);
