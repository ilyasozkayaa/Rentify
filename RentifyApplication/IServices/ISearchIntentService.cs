using RentifyApplication.Query.SearchRentals;
using RentifyApplication.Query.SearchRentals.SearchCriteria;

namespace RentifyApplication.IServices;

public interface ISearchIntentService
{
    Task<SearchIntent> CreateIntentAsync(SearchRentalsQuery query, CancellationToken cancellationToken);
}
