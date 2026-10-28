using System.Text.Json;
using RentifyApplication.IRepositories;
using RentifyApplication.IServices;
using RentifyApplication.Query.GetRentalListing;
using RentifyApplication.Query.SearchRentals.SearchCriteria;
using RentifyDomain.Entities;
using RentifyDomain.Enum;

namespace RentifyUnitTests.Query;

public sealed class GetRentalListingQueryHandlerTests
{
    [Fact]
    public async Task Handle_should_return_listing_details_and_all_permanent_photos()
    {
        using var attributes = JsonDocument.Parse("{\"bedrooms\":2}");
        var listing = new RentableProduct
        {
            Id = 31,
            Title = "Sea-view apartment",
            Description = "Near the beach",
            RentalType = (int)RentalType.Property,
            CityCode = (int)CityCode.Izmir,
            Price = 10000.00m,
            Currency = "TRY",
            Status = (int)RentableProductStatus.Active,
            Owner = new User { FirstName = "Deniz", LastName = "Host", Email = "private@example.test" },
            Attributes = attributes,
            Images =
            [
                new RentableProductImage { UploadId = Guid.NewGuid(), StorageKey = "products/31/cover.jpg", SortOrder = 0, IsPrimary = true },
                new RentableProductImage { UploadId = Guid.NewGuid(), StorageKey = "products/31/living-room.jpg", SortOrder = 1 }
            ]
        };
        var storage = new FakeImageStorage();
        var handler = new GetRentalListingQueryHandler(new FakeProductRepository(listing), storage);

        var result = await handler.Handle(new GetRentalListingQuery(31), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(10000.00m, result.Price);
        Assert.Equal("Izmir", result.City);
        Assert.Equal("Deniz", result.OwnerFirstName);
        Assert.Equal(2, result.Images.Count);
        Assert.Contains(result.Images, image => image.IsPrimary);
        Assert.Equal(2, storage.RequestedKeys.Count);
        Assert.DoesNotContain("private@example.test", JsonSerializer.Serialize(result));
    }

    [Fact]
    public async Task Handle_should_not_expose_a_pending_listing()
    {
        var pending = new RentableProduct { Id = 32, Status = (int)RentableProductStatus.Pending };
        var handler = new GetRentalListingQueryHandler(new FakeProductRepository(pending), new FakeImageStorage());

        var result = await handler.Handle(new GetRentalListingQuery(32), CancellationToken.None);

        Assert.Null(result);
    }

    private sealed class FakeProductRepository(RentableProduct listing) : IRentableProductRepository
    {
        public Task<RentableProduct?> GetActiveWithOwnerAndImagesAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<RentableProduct?>(listing.Id == id && listing.Status == (int)RentableProductStatus.Active ? listing : null);
        public Task<List<RentableProduct>> SearchAsync(SearchIntent searchIntent, CancellationToken cancellationToken = default) => Task.FromResult(new List<RentableProduct>());
        public Task<List<RentableProduct>> GetPendingAsync(int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult(new List<RentableProduct>());
        public Task<List<RentableProduct>> GetPendingByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default) => Task.FromResult(new List<RentableProduct>());
        public Task<RentableProduct?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<RentableProduct?>(null);
        public Task<RentableProduct?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<RentableProduct?>(null);
        public Task<List<RentableProduct>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<RentableProduct>());
        public Task AddAsync(RentableProduct entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Update(RentableProduct entity) { }
        public void Remove(RentableProduct entity) { }
    }

    private sealed class FakeImageStorage : IImageStorage
    {
        public List<string> RequestedKeys { get; } = [];
        public Task<string> CreatePresignedDownloadAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            RequestedKeys.Add(storageKey);
            return Task.FromResult($"https://images.example.test/{storageKey}");
        }
        public Task<PresignedImageUpload> CreatePresignedUploadAsync(string storageKey, string contentType, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ObjectExistsAsync(string storageKey, long expectedSize, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task PromoteAsync(string temporaryStorageKey, string permanentStorageKey, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ValidateImageContentAsync(string storageKey, string contentType, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
