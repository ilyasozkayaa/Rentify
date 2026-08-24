using MediatR;

namespace RentifyApplication.Query;

public sealed record GetHealthStatusQuery : IRequest<string>;
