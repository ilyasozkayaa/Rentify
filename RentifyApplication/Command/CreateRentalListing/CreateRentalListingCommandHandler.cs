using System.Text.Json;
using MediatR;
using RentifyApplication.Exceptions;
using RentifyApplication.Exceptions.Enums;
using RentifyApplication.IRepositories;
using RentifyDomain.Entities;
using RentifyDomain.Enum;

namespace RentifyApplication.Command.CreateRentalListing;

public sealed class CreateRentalListingCommandHandler : IRequestHandler<CreateRentalListingCommand, CreateRentalListingResponse>
{
    private readonly IRentableProductRepository _rentableProductRepository;
    private readonly IRentableProductImageRepository _imageRepository;
    private readonly IOutboxRepository _outboxRepository;

    public CreateRentalListingCommandHandler(IRentableProductRepository rentableProductRepository, IRentableProductImageRepository imageRepository, IOutboxRepository outboxRepository)
    {
        _rentableProductRepository = rentableProductRepository;
        _imageRepository = imageRepository;
        _outboxRepository = outboxRepository;
    }

    public async Task<CreateRentalListingResponse> Handle(CreateRentalListingCommand command, CancellationToken cancellationToken)
    {
        var rentableProduct = new RentableProduct
        {
            OwnerUserId = command.OwnerUserId,
            RentalType = command.RentalType,
            CityCode = command.CityCode,
            District = string.IsNullOrWhiteSpace(command.District) ? null : command.District.Trim(),
            Title = command.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim(),
            Price = command.Price,
            Currency = Enum.Parse<Currency>(command.Currency, true).ToString(),
            Attributes = JsonDocument.Parse(command.Attributes.GetRawText()),
            Status = (int)RentableProductStatus.Pending
        };

        await _rentableProductRepository.AddAsync(rentableProduct, cancellationToken);

        if (command.ImageUploadIds is { Count: > 0 } imageIds)
        {
            var images = await _imageRepository.GetPendingUploadsAsync(command.OwnerUserId, imageIds, cancellationToken);

            if (images.Count != imageIds.Count || images.Select(image => image.UploadBatchId).Distinct().Count() != 1 || images.Any(image => image.ExpiresAt <= DateTime.UtcNow))
                throw new BusinessException("One or more image uploads are invalid or expired.", BusinessErrorCode.InvalidImageUpload);

            var sortOrders = imageIds.Select((uploadId, index) => new { uploadId, index }).ToDictionary(x => x.uploadId, x => x.index);

            foreach (var image in images)
            {
                var sortOrder = sortOrders[image.UploadId];
                image.RentableProduct = rentableProduct;
                image.SortOrder = sortOrder;
                image.IsPrimary = sortOrder == 0;
            }

            var payload = JsonSerializer.Serialize(new { ImageUploadIds = imageIds });

            await _outboxRepository.AddAsync(new OutboxMessage
            {
                Type = (int)OutboxMessageType.RentalListingImagesProcessing,
                AggregateType = (int)OutboxAggregateType.RentableProduct,
                AggregateId = null,
                Payload = payload
            }, cancellationToken);
        }

        return new CreateRentalListingResponse(rentableProduct.Title, (RentableProductStatus)rentableProduct.Status, rentableProduct.CreatedAt);
    }
}