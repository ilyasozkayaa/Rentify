namespace RentifyApplication.Query.SearchRentals;

public sealed record SearchRentalsResponse(IReadOnlyCollection<RentalSearchResult> Results);
