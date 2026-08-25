using RentifyApplication.Query.SearchRentals.SearchCriteria;

namespace RentifyApplication.IServices;

public interface ISearchIntentService
{
    Task<SearchIntent> CreateIntentAsync(string query, CancellationToken cancellationToken);
}
