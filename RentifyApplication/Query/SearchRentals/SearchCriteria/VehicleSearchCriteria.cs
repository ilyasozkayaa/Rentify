using Microsoft.VisualBasic.FileIO;
using RentifyDomain.Enum;

namespace RentifyApplication.Query.SearchRentals.SearchCriteria;

public sealed record VehicleSearchCriteria
(
    string? Brand,
    string? Model,
    int? ModelYear,
    TransmissionType? Transmission,
    FuelType? FuelType,
    int? Seats
);
