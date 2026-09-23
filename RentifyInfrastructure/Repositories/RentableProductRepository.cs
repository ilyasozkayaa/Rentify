using Microsoft.EntityFrameworkCore;
using RentifyApplication.IRepositories;
using RentifyApplication.Query.SearchRentals.SearchCriteria;
using RentifyDomain.Entities;
using RentifyDomain.Enum;
using RentifyInfrastructure.Persistence;

namespace RentifyInfrastructure.Repositories;

public sealed class RentableProductRepository : Repository<RentableProduct>, IRentableProductRepository
{
    public RentableProductRepository(RentifyDbContext context) : base(context) { }

    public async Task<List<RentableProduct>> SearchAsync(SearchIntent searchIntent, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Where(x => x.Status == (int)RentableProductStatus.Active)
            .Where(x => x.RentalType == (int)searchIntent.RentalType)
            .Where(x => x.CityCode == searchIntent.CityCode)
            .Where(x => x.Currency == searchIntent.Currency)
            .Where(x => !x.Rents.Any(r =>
            r.Status == (int)RentStatus.Confirmed &&
            r.StartDate < searchIntent.EndDate!.Value &&
            r.EndDate > searchIntent.StartDate!.Value)); ;

        if (searchIntent.MinPrice.HasValue)
            query = query.Where(x => x.Price >= searchIntent.MinPrice.Value);

        if (searchIntent.MaxPrice.HasValue)
            query = query.Where(x => x.Price <= searchIntent.MaxPrice.Value);

        return await query.OrderBy(x => x.Id).Skip((searchIntent.Page - 1) * 1000).Take(1001).ToListAsync(cancellationToken);
    }

    public async Task<List<RentableProduct>> GetPendingAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.Status == (int)RentableProductStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<RentableProduct>> GetPendingByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => ids.Contains(x.Id) && x.Status == (int)RentableProductStatus.Pending)
            .ToListAsync(cancellationToken);
    }
}
