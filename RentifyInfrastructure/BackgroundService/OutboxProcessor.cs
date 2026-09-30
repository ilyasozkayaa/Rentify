using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RentifyApplication.IRepositories;
using RentifyApplication.IServices;
using RentifyDomain.Entities;
using RentifyDomain.Enum;

namespace RentifyInfrastructure.BackgroundServices;

public sealed class OutboxProcessor : BackgroundService
{
    private const int BatchSize = 20;
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(15);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing outbox messages.");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var imageRepository = scope.ServiceProvider.GetRequiredService<IRentableProductImageRepository>();
        var imageStorage = scope.ServiceProvider.GetRequiredService<IImageStorage>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var messages = await outboxRepository.GetPendingAsync(BatchSize, cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await ProcessMessageAsync(message, imageRepository, imageStorage, cancellationToken);

                message.ProcessedAt = DateTime.UtcNow;
                message.Error = null;

                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;

                _logger.LogError(ex, "Failed to process outbox message {OutboxMessageId}.", message.Id);

                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private static async Task ProcessMessageAsync(OutboxMessage message, IRentableProductImageRepository imageRepository, IImageStorage imageStorage, CancellationToken cancellationToken)
    {
        if (message.Type != (int)OutboxMessageType.RentalListingImagesProcessing)
            throw new InvalidOperationException($"Unsupported outbox message type: {message.Type}.");

        if (string.IsNullOrWhiteSpace(message.Payload))
            throw new InvalidOperationException("Outbox message payload is empty.");

        var payload = JsonSerializer.Deserialize<ImageProcessingPayload>(message.Payload);

        if (payload is null || payload.ImageUploadIds.Count == 0)
            throw new InvalidOperationException("Outbox message payload is invalid.");

        var images = await imageRepository.GetByUploadIdsAsync(payload.ImageUploadIds, cancellationToken);

        if (images.Count != payload.ImageUploadIds.Count)
            throw new InvalidOperationException("One or more image uploads could not be found.");

        foreach (var image in images)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!image.StorageKey.StartsWith("temporary/", StringComparison.Ordinal))
                continue;

            if (!await imageStorage.ObjectExistsAsync(image.StorageKey, image.FileSize, cancellationToken))
                throw new InvalidOperationException($"Uploaded image was not found: {image.UploadId}.");

            var permanentKey = $"products/{image.OwnerUserId}/{Guid.NewGuid():N}/{image.UploadId:N}{Path.GetExtension(image.StorageKey)}";

            if (!await imageStorage.ValidateImageContentAsync(image.StorageKey, image.ContentType, cancellationToken))
                throw new InvalidOperationException($"Uploaded file is not a valid {image.ContentType} image: {image.UploadId}.");

            await imageStorage.PromoteAsync(image.StorageKey, permanentKey, cancellationToken);
            image.StorageKey = permanentKey;
        }
    }

    private sealed record ImageProcessingPayload(IReadOnlyList<Guid> ImageUploadIds);
}