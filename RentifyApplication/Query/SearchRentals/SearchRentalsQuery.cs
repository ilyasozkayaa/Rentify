using MediatR;

namespace RentifyApplication.Query.SearchRentals;

public sealed record SearchRentalsQuery(string Query) : IRequest<SearchRentalsResponse>;
