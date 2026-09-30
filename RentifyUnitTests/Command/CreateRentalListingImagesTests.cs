using System.Text.Json;
using RentifyApplication.Command.CreateRentalListing;
using RentifyApplication.Exceptions;
using RentifyApplication.IRepositories;
using RentifyApplication.Query.SearchRentals.SearchCriteria;
using RentifyDomain.Entities;
using RentifyDomain.Enum;

namespace RentifyUnitTests.Command;

public sealed class CreateRentalListingImagesTests
{
    [Fact]
    public async Task Handle_attaches_owned_uploads_and_sets_one_primary_image()
    {
        using var attributes = JsonDocument.Parse("{}");
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var batchId = Guid.NewGuid();
        var pending = ids.Select((id, index) => new RentableProductImage
        {
            UploadId = id,
            UploadBatchId = batchId,
            OwnerUserId = 42,
            StorageKey = $"temporary/42/{id:N}.jpg",
            ContentType = "image/jpeg",
            FileSize = 123,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            SortOrder = index
        }).ToList();
        var images = new FakeImageRepository(pending);
        var outbox = new FakeOutboxRepository();
        var handler = new CreateRentalListingCommandHandler(new FakeProductRepository(), images, outbox);
        var command = new CreateRentalListingCommand(42, 2, 7, null, "Apartment", null, 100, "TRY", attributes.RootElement, ids);

        await handler.Handle(command, CancellationToken.None);

        Assert.Equal(ids[0], pending.Single(image => image.IsPrimary).UploadId);
        Assert.Equal(0, pending.Single(image => image.IsPrimary).SortOrder);
        Assert.Single(pending, image => image.IsPrimary);
        Assert.Single(outbox.Messages);
        Assert.Equal((int)OutboxMessageType.RentalListingImagesProcessing, outbox.Messages[0].Type);
        Assert.Equal((int)OutboxAggregateType.RentableProduct, outbox.Messages[0].AggregateType);
        Assert.Null(outbox.Messages[0].AggregateId);
        Assert.NotNull(outbox.Messages[0].Payload);
    }

    [Fact]
    public async Task Handle_rejects_upload_ids_not_found_in_the_owner_batch()
    {
        using var attributes = JsonDocument.Parse("{}");
        var handler = new CreateRentalListingCommandHandler(new FakeProductRepository(), new FakeImageRepository([]), new FakeOutboxRepository());
        var command = new CreateRentalListingCommand(42, 2, 7, null, "Apartment", null, 100, "TRY", attributes.RootElement, [Guid.NewGuid()]);

        await Assert.ThrowsAsync<BusinessException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_rejects_upload_ids_from_different_batches()
    {
        using var attributes = JsonDocument.Parse("{}");
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var pending = ids.Select(id => new RentableProductImage
        {
            UploadId = id,
            UploadBatchId = Guid.NewGuid(),
            OwnerUserId = 42,
            StorageKey = $"temporary/42/{id:N}.jpg",
            ContentType = "image/jpeg",
            FileSize = 123,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        }).ToList();
        var outbox = new FakeOutboxRepository();
        var handler = new CreateRentalListingCommandHandler(new FakeProductRepository(), new FakeImageRepository(pending), outbox);
        var command = new CreateRentalListingCommand(42, 2, 7, null, "Apartment", null, 100, "TRY", attributes.RootElement, ids);

        await Assert.ThrowsAsync<BusinessException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Empty(outbox.Messages);
    }

    private sealed class FakeProductRepository : IRentableProductRepository
    {
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

    private sealed class FakeImageRepository(List<RentableProductImage> pending) : IRentableProductImageRepository
    {
        public Task<List<RentableProductImage>> GetPendingUploadsAsync(int ownerUserId, IReadOnlyCollection<Guid> uploadIds, CancellationToken cancellationToken = default) => Task.FromResult(pending.Where(x => x.OwnerUserId == ownerUserId && uploadIds.Contains(x.UploadId)).ToList());
        public Task<List<RentableProductImage>> GetByUploadIdsAsync(IReadOnlyCollection<Guid> uploadIds, CancellationToken cancellationToken = default) => Task.FromResult(pending.Where(x => uploadIds.Contains(x.UploadId)).ToList());
        public Task<RentableProductImage?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<RentableProductImage?>(null);
        public Task<List<RentableProductImage>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(pending);
        public Task AddAsync(RentableProductImage entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Update(RentableProductImage entity) { }
        public void Remove(RentableProductImage entity) { }
    }

    private sealed class FakeOutboxRepository : IOutboxRepository
    {
        public List<OutboxMessage> Messages { get; } = [];

        public Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }

        public Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default) => Task.FromResult(Messages.Where(x => x.ProcessedAt == null).Take(batchSize).ToList());
    }
}