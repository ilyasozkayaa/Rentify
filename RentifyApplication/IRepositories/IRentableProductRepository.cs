using RentifyApplication.Query.SearchRentals.SearchCriteria;
using RentifyDomain.Entities;

namespace RentifyApplication.IRepositories;

public interface IRentableProductRepository : IRepository<RentableProduct>
{
    Task<List<RentableProduct>> SearchAsync(SearchIntent searchIntent, CancellationToken cancellationToken = default);
}
