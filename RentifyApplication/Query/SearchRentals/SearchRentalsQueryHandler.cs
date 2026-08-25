using MediatR;
using RentifyApplication.IServices;

namespace RentifyApplication.Query.SearchRentals;

public sealed class SearchRentalsQueryHandler : IRequestHandler<SearchRentalsQuery, SearchRentalsResponse>
{
    private readonly ISearchIntentService _searchIntentService;

    public SearchRentalsQueryHandler(ISearchIntentService searchIntentService)
    {
        _searchIntentService = searchIntentService;
    }

    public async Task<SearchRentalsResponse> Handle(SearchRentalsQuery request, CancellationToken cancellationToken)
    {
        var searchIntent = await _searchIntentService.CreateIntentAsync(request.Query, cancellationToken);

        // TODO: Search strategy + repository
        return new SearchRentalsResponse(Array.Empty<RentalSearchResult>());
    }
}
