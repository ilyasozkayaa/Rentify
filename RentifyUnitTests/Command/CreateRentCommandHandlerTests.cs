using RentifyApplication.Command.CreateRent;
using RentifyApplication.Exceptions;
using RentifyApplication.Exceptions.Enums;
using RentifyApplication.IRepositories;
using RentifyApplication.Query.SearchRentals.SearchCriteria;
using RentifyDomain.Entities;
using RentifyDomain.Enum;

namespace RentifyUnitTests.Command;

public sealed class CreateRentCommandHandlerTests
{
    [Fact]
    public async Task Handle_should_create_a_confirmed_rent_and_calculate_daily_total()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var product = new RentableProduct
        {
            Id = 7,
            OwnerUserId = 99,
            Price = 125.50m,
            Currency = "TRY",
            Status = (int)RentableProductStatus.Active
        };
        var rentRepository = new FakeRentRepository();
        var handler = CreateHandler(new FakeRentableProductRepository(product), rentRepository);

        var response = await handler.Handle(new CreateRentCommand(42, 7, today.AddDays(1), today.AddDays(4)), CancellationToken.None);

        var rent = Assert.Single(rentRepository.Rents);
        Assert.Equal(42, rent.RenterUserId);
        Assert.Equal(7, rent.RentableProductId);
        Assert.Equal(376.50m, rent.TotalPrice);
        Assert.Equal((int)RentStatus.Confirmed, rent.Status);
        Assert.Equal("TRY", response.Currency);
        Assert.Equal(376.50m, response.TotalPrice);
    }

    [Fact]
    public async Task Handle_should_reject_overlapping_confirmed_rent_without_adding_a_record()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var rentRepository = new FakeRentRepository { HasOverlap = true };
        var handler = CreateHandler(
            new FakeRentableProductRepository(new RentableProduct { Id = 7, Status = (int)RentableProductStatus.Active, Price = 100 }),
            rentRepository);

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            handler.Handle(new CreateRentCommand(42, 7, today.AddDays(1), today.AddDays(3)), CancellationToken.None));

        Assert.Equal(BusinessErrorCode.RentalDatesUnavailable, exception.Code);
        Assert.Empty(rentRepository.Rents);
    }

    [Fact]
    public async Task Handle_should_reject_a_non_active_listing()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var repository = new FakeRentRepository();
        var handler = CreateHandler(
            new FakeRentableProductRepository(new RentableProduct { Id = 7, Status = (int)RentableProductStatus.Pending, Price = 100 }),
            repository);

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            handler.Handle(new CreateRentCommand(42, 7, today.AddDays(1), today.AddDays(3)), CancellationToken.None));

        Assert.Equal(BusinessErrorCode.RentableProductUnavailable, exception.Code);
        Assert.Empty(repository.Rents);
    }

    private static CreateRentCommandHandler CreateHandler(IRentableProductRepository productRepository, IRentRepository rentRepository)
    {
        return new CreateRentCommandHandler(productRepository, rentRepository);
    }

    private sealed class FakeRentableProductRepository(RentableProduct? product) : IRentableProductRepository
    {
        public Task<RentableProduct?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult(product?.Id == id ? product : null);
        public Task<List<RentableProduct>> SearchAsync(SearchIntent searchIntent, CancellationToken cancellationToken = default) => Task.FromResult(product is null ? new List<RentableProduct>() : [product]);
        public Task<List<RentableProduct>> GetPendingAsync(int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult(new List<RentableProduct>());
        public Task<List<RentableProduct>> GetPendingByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default) => Task.FromResult(new List<RentableProduct>());
        public Task<RentableProduct?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult(product?.Id == id ? product : null);
        public Task<List<RentableProduct>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(product is null ? new List<RentableProduct>() : [product]);
        public Task AddAsync(RentableProduct entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Update(RentableProduct entity) { }
        public void Remove(RentableProduct entity) { }
    }

    private sealed class FakeRentRepository : IRentRepository
    {
        public List<Rent> Rents { get; } = [];
        public bool HasOverlap { get; init; }

        public Task<bool> HasConfirmedOverlapAsync(int rentableProductId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default) => Task.FromResult(HasOverlap);
        public Task<Rent?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult(Rents.SingleOrDefault(x => x.Id == id));
        public Task<List<Rent>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(Rents);
        public Task AddAsync(Rent entity, CancellationToken cancellationToken = default)
        {
            Rents.Add(entity);
            return Task.CompletedTask;
        }
        public void Update(Rent entity) { }
        public void Remove(Rent entity) => Rents.Remove(entity);
    }
}