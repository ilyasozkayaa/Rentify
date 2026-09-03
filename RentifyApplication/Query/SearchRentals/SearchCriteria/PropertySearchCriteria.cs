namespace RentifyApplication.Query.SearchRentals.SearchCriteria;

public sealed record PropertySearchCriteria
(
    bool? SeaView,
    bool? Detached,
    int? Bedrooms,
    bool? Pool,
    int? Bathrooms,
    bool? Furnished,
    bool? Garden
);
