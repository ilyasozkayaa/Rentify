using MediatR;

namespace RentifyApplication.Query.GetMyRents;

public sealed record GetMyRentsQuery(int RenterUserId, int Page, int PageSize) : IRequest<MyRentsResponse>;
