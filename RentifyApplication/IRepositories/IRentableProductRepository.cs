using RentifyApplication.Query.SearchRentals.SearchCriteria;
using RentifyDomain.Entities;

namespace RentifyApplication.IRepositories;

public interface IRentableProductRepository : IRepository<RentableProduct>
{
    Task<List<RentableProduct>> SearchAsync(SearchIntent searchIntent, CancellationToken cancellationToken = default);
    Task<List<RentableProduct>> GetPendingAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<RentableProduct>> GetPendingByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default);
}
