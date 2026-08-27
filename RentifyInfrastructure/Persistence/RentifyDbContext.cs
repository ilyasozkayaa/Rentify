using Microsoft.EntityFrameworkCore;
using RentifyDomain.Entities;

namespace RentifyInfrastructure.Persistence;

public sealed class RentifyDbContext : DbContext
{
    public RentifyDbContext(DbContextOptions<RentifyDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RentableProduct> RentableProducts => Set<RentableProduct>();
    public DbSet<Rent> Rents => Set<Rent>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RentifyDbContext).Assembly);
    }
}
