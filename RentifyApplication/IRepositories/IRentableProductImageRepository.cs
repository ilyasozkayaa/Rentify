using RentifyDomain.Entities;

namespace RentifyApplication.IRepositories;

public interface IRentableProductImageRepository : IRepository<RentableProductImage>
{
    Task<List<RentableProductImage>> GetPendingUploadsAsync(int ownerUserId, IReadOnlyCollection<Guid> uploadIds, CancellationToken cancellationToken = default);
    Task<List<RentableProductImage>> GetByUploadIdsAsync(IReadOnlyCollection<Guid> uploadIds, CancellationToken cancellationToken = default);
}