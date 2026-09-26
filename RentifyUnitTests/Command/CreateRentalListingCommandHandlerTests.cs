using System.Text.Json;
using RentifyApplication.Command.CreateRentalListing;
using RentifyApplication.IRepositories;
using RentifyApplication.Query.SearchRentals.SearchCriteria;
using RentifyDomain.Entities;
using RentifyDomain.Enum;

namespace RentifyUnitTests.Command;

public sealed class CreateRentalListingCommandHandlerTests
{
    [Fact]
    public async Task Handle_should_create_an_inactive_listing_owned_by_the_authenticated_user()
    {
        using var attributes = JsonDocument.Parse("""{"bedrooms":2}""");
        var repository = new FakeRentableProductRepository();
        var handler = new CreateRentalListingCommandHandler(repository);
        var command = new CreateRentalListingCommand(42, 2, 7, "  Konyaaltı ", "  Deniz manzaralı daire  ", "  Açıklama  ", 1250.50m, "try", attributes.RootElement);

        var response = await handler.Handle(command, CancellationToken.None);

        var listing = Assert.Single(repository.Listings);
        Assert.Equal(42, listing.OwnerUserId);
        Assert.Equal("Deniz manzaralı daire", listing.Title);
        Assert.Equal("Konyaaltı", listing.District);
        Assert.Equal("Açıklama", listing.Description);
        Assert.Equal("TRY", listing.Currency);
        Assert.Equal((int)RentableProductStatus.Pending, listing.Status);
        Assert.Equal(RentableProductStatus.Pending, response.Status);
        Assert.Equal("""{"bedrooms":2}""", listing.Attributes!.RootElement.GetRawText());
        Assert.Equal(0, listing.Id);
    }

    private sealed class FakeRentableProductRepository : IRentableProductRepository
    {
        public List<RentableProduct> Listings { get; } = [];

        public Task<List<RentableProduct>> SearchAsync(SearchIntent searchIntent, CancellationToken cancellationToken = default) => Task.FromResult(Listings);
        public Task<List<RentableProduct>> GetPendingAsync(int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult(Listings.Where(x => x.Status == (int)RentableProductStatus.Pending).ToList());
        public Task<List<RentableProduct>> GetPendingByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default) => Task.FromResult(Listings.Where(x => ids.Contains(x.Id) && x.Status == (int)RentableProductStatus.Pending).ToList());
        public Task<RentableProduct?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult(Listings.SingleOrDefault(x => x.Id == id));
        public Task<RentableProduct?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult(Listings.SingleOrDefault(x => x.Id == id));
        public Task<List<RentableProduct>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(Listings);
        public Task AddAsync(RentableProduct entity, CancellationToken cancellationToken = default)
        {
            Listings.Add(entity);
            return Task.CompletedTask;
        }
        public void Update(RentableProduct entity) { }
        public void Remove(RentableProduct entity) => Listings.Remove(entity);
    }

}