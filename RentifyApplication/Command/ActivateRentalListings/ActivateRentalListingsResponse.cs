namespace RentifyApplication.Command.ActivateRentalListings;

public sealed record ActivateRentalListingsResponse(IReadOnlyCollection<int> ActivatedIds, int ActivatedCount);
