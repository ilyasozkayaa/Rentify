using Microsoft.EntityFrameworkCore;
using RentifyApplication.IRepositories;
using RentifyInfrastructure.Persistence;

namespace RentifyInfrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly RentifyDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(RentifyDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync([id], cancellationToken);
    }

    public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(T entity)
    {
        DbSet.Update(entity);
    }

    public void Remove(T entity)
    {
        DbSet.Remove(entity);
    }
}
