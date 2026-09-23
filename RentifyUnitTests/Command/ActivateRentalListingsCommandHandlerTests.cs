using RentifyApplication.Command.ActivateRentalListings;
using RentifyApplication.Exceptions;
using RentifyApplication.Exceptions.Enums;
using RentifyApplication.IRepositories;
using RentifyApplication.Query.SearchRentals.SearchCriteria;
using RentifyDomain.Entities;
using RentifyDomain.Enum;

namespace RentifyUnitTests.Command;

public sealed class ActivateRentalListingsCommandHandlerTests
{
    [Fact]
    public async Task Handle_should_activate_all_selected_pending_listings()
    {
        var first = PendingListing(10);
        var second = PendingListing(20);
        var repository = new FakeRentableProductRepository([first, second]);
        var handler = new ActivateRentalListingsCommandHandler(repository);

        var response = await handler.Handle(new ActivateRentalListingsCommand([20, 10]), CancellationToken.None);

        Assert.Equal([20, 10], response.ActivatedIds);
        Assert.Equal(2, response.ActivatedCount);
        Assert.All(repository.Listings, listing => Assert.Equal((int)RentableProductStatus.Active, listing.Status));
    }

    [Fact]
    public async Task Handle_should_not_change_any_listing_if_one_id_is_not_pending()
    {
        var pending = PendingListing(10);
        var repository = new FakeRentableProductRepository(
        [
            pending,
            new RentableProduct { Id = 20, Status = (int)RentableProductStatus.Active }
        ]);
        var handler = new ActivateRentalListingsCommandHandler(repository);

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            handler.Handle(new ActivateRentalListingsCommand([10, 20]), CancellationToken.None));

        Assert.Equal(BusinessErrorCode.RentalListingsNotPending, exception.Code);
        Assert.Equal((int)RentableProductStatus.Pending, pending.Status);
    }

    private static RentableProduct PendingListing(int id) => new()
    {
        Id = id,
        Status = (int)RentableProductStatus.Pending
    };

    private sealed class FakeRentableProductRepository : IRentableProductRepository
    {
        private readonly List<RentableProduct> _listings;

        public FakeRentableProductRepository(List<RentableProduct> listings)
        {
            _listings = listings;
        }

        public List<RentableProduct> Listings => _listings;

        public Task<List<RentableProduct>> SearchAsync(SearchIntent searchIntent, CancellationToken cancellationToken = default) => Task.FromResult(_listings);
        public Task<List<RentableProduct>> GetPendingAsync(int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult(_listings.Where(IsPending).Skip((page - 1) * pageSize).Take(pageSize + 1).ToList());
        public Task<List<RentableProduct>> GetPendingByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default) => Task.FromResult(_listings.Where(x => ids.Contains(x.Id) && IsPending(x)).ToList());
        public Task<RentableProduct?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult(_listings.SingleOrDefault(x => x.Id == id));
        public Task<List<RentableProduct>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(_listings);
        public Task AddAsync(RentableProduct entity, CancellationToken cancellationToken = default)
        {
            _listings.Add(entity);
            return Task.CompletedTask;
        }
        public void Update(RentableProduct entity) { }
        public void Remove(RentableProduct entity) => _listings.Remove(entity);

        private static bool IsPending(RentableProduct listing) => listing.Status == (int)RentableProductStatus.Pending;
    }
}
