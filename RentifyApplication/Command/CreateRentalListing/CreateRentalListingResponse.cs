using RentifyDomain.Enum;

namespace RentifyApplication.Command.CreateRentalListing;

public sealed record CreateRentalListingResponse(string Title, RentableProductStatus Status, DateTime CreatedAt);
