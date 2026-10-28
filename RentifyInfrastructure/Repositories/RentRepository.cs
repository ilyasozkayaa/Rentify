using RentifyApplication.IRepositories;
using RentifyDomain.Entities;
using RentifyInfrastructure.Persistence;
using RentifyDomain.Enum;
using Microsoft.EntityFrameworkCore;
using RentifyApplication.Query.GetMyRents;

namespace RentifyInfrastructure.Repositories;

public sealed class RentRepository : Repository<Rent>, IRentRepository
{
    public RentRepository(RentifyDbContext context) : base(context)
    {
    }

    public Task<bool> HasConfirmedOverlapAsync(int rentableProductId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        return DbSet.AnyAsync(rent =>
            rent.RentableProductId == rentableProductId &&
            rent.Status == (int)RentStatus.Confirmed &&
            rent.StartDate < endDate &&
            rent.EndDate > startDate,
            cancellationToken);
    }

    public Task<List<MyRentRecord>> GetByRenterAsync(int renterUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return DbSet
            .AsNoTracking()
            .Where(rent => rent.RenterUserId == renterUserId)
            .OrderByDescending(rent => rent.CreatedAt)
            .ThenByDescending(rent => rent.Id)
            .Skip((page - 1) * pageSize)
            .Select(rent => new MyRentRecord(
                rent.Id,
                rent.RentableProductId,
                rent.RentableProduct.Title,
                rent.RentableProduct.RentalType,
                rent.RentableProduct.CityCode,
                rent.RentableProduct.District,
                rent.RentableProduct.Images
                    .Where(image => !image.StorageKey.StartsWith("temporary/"))
                    .OrderByDescending(image => image.IsPrimary)
                    .ThenBy(image => image.SortOrder)
                    .Select(image => image.StorageKey)
                    .FirstOrDefault(),
                rent.StartDate,
                rent.EndDate,
                rent.RentableProduct.Price,
                rent.TotalPrice,
                rent.RentableProduct.Currency,
                rent.Status,
                rent.CreatedAt))
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);
    }
}
