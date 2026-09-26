using RentifyApplication.IRepositories;
using RentifyDomain.Entities;
using RentifyInfrastructure.Persistence;
using RentifyDomain.Enum;
using Microsoft.EntityFrameworkCore;

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
}