using RentifyApplication.Command.RejectRentalListing;
using RentifyApplication.Exceptions;
using RentifyApplication.Exceptions.Enums;
using RentifyApplication.IRepositories;
using RentifyApplication.Query.SearchRentals.SearchCriteria;
using RentifyDomain.Entities;
using RentifyDomain.Enum;

namespace RentifyUnitTests.Command;

public sealed class RejectRentalListingCommandHandlerTests
{
    [Fact]
    public async Task Handle_should_reject_pending_listing_and_store_reason()
    {
        var listing = new RentableProduct { Id = 12, Title = "A home", Status = (int)RentableProductStatus.Pending };
        var repository = new FakeRentableProductRepository(listing);
        var handler = new RejectRentalListingCommandHandler(repository);

        var response = await handler.Handle(new RejectRentalListingCommand(12, "  Please add clearer photos.  "), CancellationToken.None);

        Assert.Equal((int)RentableProductStatus.Rejected, listing.Status);
        Assert.Equal("Please add clearer photos.", listing.RejectionReason);
        Assert.Equal("A home", response.Title);
        Assert.Equal("Please add clearer photos.", response.Reason);
    }

    [Fact]
    public async Task Handle_should_reject_non_pending_listing_without_changing_it()
    {
        var listing = new RentableProduct { Id = 12, Status = (int)RentableProductStatus.Active };
        var handler = new RejectRentalListingCommandHandler(new FakeRentableProductRepository(listing));

        var exception = await Assert.ThrowsAsync<BusinessException>(() => handler.Handle(new RejectRentalListingCommand(12, "Not suitable"), CancellationToken.None));

        Assert.Equal(BusinessErrorCode.RentalListingsNotPending, exception.Code);
        Assert.Equal((int)RentableProductStatus.Active, listing.Status);
        Assert.Null(listing.RejectionReason);
    }

    private sealed class FakeRentableProductRepository(RentableProduct listing) : IRentableProductRepository
    {
        public Task<RentableProduct?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<RentableProduct?>(listing.Id == id ? listing : null);
        public Task<RentableProduct?> GetActiveWithOwnerAndImagesAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<RentableProduct?>(null);
        public Task<List<RentableProduct>> SearchAsync(SearchIntent searchIntent, CancellationToken cancellationToken = default) => Task.FromResult(new List<RentableProduct>());
        public Task<List<RentableProduct>> GetPendingAsync(int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult(new List<RentableProduct>());
        public Task<List<RentableProduct>> GetPendingByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default) => Task.FromResult(new List<RentableProduct>());
        public Task<RentableProduct?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<RentableProduct?>(null);
        public Task<List<RentableProduct>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<RentableProduct>());
        public Task AddAsync(RentableProduct entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Update(RentableProduct entity) { }
        public void Remove(RentableProduct entity) { }
    }
}
