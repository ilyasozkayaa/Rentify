namespace RentifyApplication.Query.SearchRentals.SearchCriteria;

public sealed record HotelSearchCriteria
(
    int? MinimumStars,
    bool? BreakfastIncluded,
    bool? Pool,
    string? RoomType
);