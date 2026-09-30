using Microsoft.EntityFrameworkCore;
using RentifyApplication.Constants;
using RentifyApplication.IRepositories;
using RentifyDomain.Entities;
using RentifyInfrastructure.Persistence;

namespace RentifyInfrastructure.Repositories;

public sealed class OutboxRepository : Repository<OutboxMessage>, IOutboxRepository
{
    public OutboxRepository(RentifyDbContext context) : base(context)
    {
    }

    public Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        return DbSet
            .Where(x => x.ProcessedAt == null && x.RetryCount < ProjectConstants.OutboxRetryCount)
            .OrderBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }
}