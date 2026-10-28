using MediatR;

namespace RentifyApplication.Query.GetRentalListing;

public sealed record GetRentalListingQuery(int Id) : IRequest<RentalListingDetails?>;
