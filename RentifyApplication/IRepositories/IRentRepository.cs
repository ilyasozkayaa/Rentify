using RentifyDomain.Entities;

namespace RentifyApplication.IRepositories;

public interface IRentRepository : IRepository<Rent>
{
    Task<bool> HasConfirmedOverlapAsync(int rentableProductId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
}