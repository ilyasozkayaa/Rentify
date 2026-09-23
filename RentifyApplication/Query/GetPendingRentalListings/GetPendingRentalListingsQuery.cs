using MediatR;

namespace RentifyApplication.Query.GetPendingRentalListings;

public sealed record GetPendingRentalListingsQuery(int Page = 1, int PageSize = 50) : IRequest<GetPendingRentalListingsResponse>;
