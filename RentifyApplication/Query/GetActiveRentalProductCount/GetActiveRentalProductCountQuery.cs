using MediatR;

namespace RentifyApplication.Query.GetActiveRentalProductCount;

public sealed record GetActiveRentalProductCountQuery : IRequest<int>;
