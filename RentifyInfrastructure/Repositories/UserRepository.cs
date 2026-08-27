using RentifyApplication.IRepositories;
using RentifyDomain.Entities;
using RentifyInfrastructure.Persistence;

namespace RentifyInfrastructure.Repositories;

public sealed class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(RentifyDbContext context) : base(context)
    {
    }
}
