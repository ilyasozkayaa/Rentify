using RentifyDomain.Entities;

namespace RentifyApplication.IRepositories;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default);
}