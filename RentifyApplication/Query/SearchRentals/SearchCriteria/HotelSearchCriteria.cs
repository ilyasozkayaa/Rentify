namespace RentifyApplication.Query.SearchRentals.SearchCriteria;

public sealed record HotelSearchCriteria
(
    int? Stars,
    bool? BreakfastIncluded,
    bool? Pool,
    bool? OpenBuffet,
    bool? AllInclusive,
    int? GuestCapacity,
    bool? Parking,
    bool? SeaView,
    bool? AirConditioning,
    bool? Wifi,
    bool? Restaurant,
    bool? Gym,
    bool? Spa
);