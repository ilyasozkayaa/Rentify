using RentifyApplication.IRepositories;
using RentifyDomain.Entities;
using RentifyInfrastructure.Persistence;

namespace RentifyInfrastructure.Repositories;

public sealed class RentableProductRepository : Repository<RentableProduct>, IRentableProductRepository
{
    public RentableProductRepository(RentifyDbContext context) : base(context)
    {
    }
}
