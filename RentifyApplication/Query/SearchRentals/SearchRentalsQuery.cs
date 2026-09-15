using MediatR;

namespace RentifyApplication.Query.SearchRentals;

public sealed record SearchRentalsQuery(string Query, int Page = 1) : IRequest<SearchRentalsResponse>;
