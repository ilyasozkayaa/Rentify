using Microsoft.EntityFrameworkCore;
using RentifyApplication.IRepositories;
using RentifyDomain.Entities;
using RentifyInfrastructure.Persistence;

namespace RentifyInfrastructure.Repositories;

public sealed class RentableProductImageRepository : Repository<RentableProductImage>, IRentableProductImageRepository
{
    public RentableProductImageRepository(RentifyDbContext context) : base(context) { }

    public Task<List<RentableProductImage>> GetPendingUploadsAsync(int ownerUserId, IReadOnlyCollection<Guid> uploadIds, CancellationToken cancellationToken = default)
    {
        return DbSet.Where(image => image.OwnerUserId == ownerUserId  && image.RentableProductId == null  && uploadIds.Contains(image.UploadId)).ToListAsync(cancellationToken);
    }
}