using RentifyApplication.IRepositories;
using RentifyApplication.IServices;
using RentifyApplication.Query.GetMyRents;
using RentifyDomain.Entities;
using RentifyDomain.Enum;

namespace RentifyUnitTests.Query;

public sealed class GetMyRentsQueryHandlerTests
{
    [Fact]
    public async Task Handle_should_return_paged_rents_with_listing_cover_and_current_status()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var repository = new FakeRentRepository
        {
            Rents =
            [
                CreateRent(4, today, RentStatus.Confirmed, "Primary"),
                CreateRent(3, today.AddDays(1), RentStatus.Pending, null),
                CreateRent(2, today.AddDays(2), RentStatus.Cancelled, null)
            ]
        };
        var imageStorage = new FakeImageStorage();
        var handler = new GetMyRentsQueryHandler(repository, imageStorage);

        var response = await handler.Handle(new GetMyRentsQuery(27, 1, 2), CancellationToken.None);

        Assert.Equal((27, 1, 2), (repository.RenterUserId, repository.Page, repository.PageSize));
        Assert.True(response.HasNextPage);
        Assert.Equal(2, response.Results.Count);
        Assert.DoesNotContain(response.Results, rent => rent.Id == 1);
        Assert.Equal(4, response.Results.First().Id);
        Assert.Equal("Listing 4", response.Results.First().ListingTitle);
        Assert.Equal("https://images.example/Primary", response.Results.First().CoverImageUrl);
        Assert.Equal(RentStatus.Confirmed, response.Results.First().Status);
        Assert.Null(response.Results.Last().CoverImageUrl);
    }

    private static MyRentRecord CreateRent(int id, DateOnly startDate, RentStatus status, string? coverKey)
    {
        return new MyRentRecord(
            id,
            id + 100,
            $"Listing {id}",
            (int)RentalType.Vehicle,
            (int)CityCode.Istanbul,
            "Kadıköy",
            coverKey,
            startDate,
            startDate.AddDays(2),
            150,
            300,
            "TRY",
            (int)status,
            DateTime.UtcNow);
    }

    private sealed class FakeRentRepository : IRentRepository
    {
        public List<MyRentRecord> Rents { get; init; } = [];
        public int RenterUserId { get; private set; }
        public int Page { get; private set; }
        public int PageSize { get; private set; }
        public Task<bool> HasConfirmedOverlapAsync(int rentableProductId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<List<MyRentRecord>> GetByRenterAsync(int renterUserId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            RenterUserId = renterUserId;
            Page = page;
            PageSize = pageSize;
            return Task.FromResult(Rents.Take(pageSize + 1).ToList());
        }
        public Task<Rent?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<Rent?>(null);
        public Task<List<Rent>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<Rent>());
        public Task AddAsync(Rent entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Update(Rent entity) { }
        public void Remove(Rent entity) { }
    }

    private sealed class FakeImageStorage : IImageStorage
    {
        public Task<string> CreatePresignedDownloadAsync(string storageKey, CancellationToken cancellationToken = default) => Task.FromResult($"https://images.example/{storageKey}");
        public Task<PresignedImageUpload> CreatePresignedUploadAsync(string storageKey, string contentType, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ObjectExistsAsync(string storageKey, long expectedSize, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task PromoteAsync(string temporaryStorageKey, string permanentStorageKey, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ValidateImageContentAsync(string storageKey, string contentType, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
