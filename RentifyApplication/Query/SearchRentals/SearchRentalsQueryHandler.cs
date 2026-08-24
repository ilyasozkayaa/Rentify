using MediatR;

namespace RentifyApplication.Query.SearchRentals;

public sealed class SearchRentalsQueryHandler : IRequestHandler<SearchRentalsQuery, SearchRentalsResponse>
{
    public async Task<SearchRentalsResponse> Handle(SearchRentalsQuery request, CancellationToken cancellationToken)
    {
        var response = new SearchRentalsResponse(
            Array.Empty<RentalSearchResult>());

        return await Task.FromResult(response);
    }
}
