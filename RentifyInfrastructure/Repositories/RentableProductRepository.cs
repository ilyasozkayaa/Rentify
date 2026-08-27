using Microsoft.EntityFrameworkCore;
using RentifyApplication.IRepositories;
using RentifyApplication.Query.SearchRentals.SearchCriteria;
using RentifyDomain.Entities;
using RentifyInfrastructure.Persistence;

namespace RentifyInfrastructure.Repositories;

public sealed class RentableProductRepository : Repository<RentableProduct>, IRentableProductRepository
{
    public RentableProductRepository(RentifyDbContext context) : base(context) { }

    public async Task<List<RentableProduct>> SearchAsync(SearchIntent searchIntent, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Where(x => x.RentalType == (int)searchIntent.RentalType)
            .Where(x => x.CityCode == searchIntent.CityCode);

        if (searchIntent.MinPrice.HasValue)
            query = query.Where(x => x.Price >= searchIntent.MinPrice.Value);

        if (searchIntent.MaxPrice.HasValue)
            query = query.Where(x => x.Price <= searchIntent.MaxPrice.Value);

        if (searchIntent.MinPrice.HasValue || searchIntent.MaxPrice.HasValue)
            query = query.Where(x => x.Currency == searchIntent.Currency);

        return await query.ToListAsync(cancellationToken);
    }
}
