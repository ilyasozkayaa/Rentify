using RentifyApplication.IRepositories;
using RentifyDomain.Entities;
using RentifyInfrastructure.Persistence;

namespace RentifyInfrastructure.Repositories;

public sealed class RentRepository : Repository<Rent>, IRentRepository
{
    public RentRepository(RentifyDbContext context) : base(context)
    {
    }
}
