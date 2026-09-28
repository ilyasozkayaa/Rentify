using System.Text.Json;
using MediatR;
using RentifyApplication.IRepositories;
using RentifyApplication.IServices;
using RentifyApplication.Exceptions;
using RentifyApplication.Exceptions.Enums;
using RentifyDomain.Entities;
using RentifyDomain.Enum;

namespace RentifyApplication.Command.CreateRentalListing;

public sealed class CreateRentalListingCommandHandler : IRequestHandler<CreateRentalListingCommand, CreateRentalListingResponse>
{
    private readonly IRentableProductRepository _rentableProductRepository;
    private readonly IRentableProductImageRepository _imageRepository;
    private readonly IImageStorage _imageStorage;

    public CreateRentalListingCommandHandler(IRentableProductRepository rentableProductRepository, IRentableProductImageRepository imageRepository, IImageStorage imageStorage)
    {
        _rentableProductRepository = rentableProductRepository;
        _imageRepository = imageRepository;
        _imageStorage = imageStorage;
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
                if (!await _imageStorage.ObjectExistsAsync(image.StorageKey, image.FileSize, cancellationToken))
                {
                    throw new BusinessException("One or more uploaded images are missing or have an unexpected size.", BusinessErrorCode.InvalidImageUpload);
                }

                var sortOrder = sortOrders[image.UploadId];
                var permanentKey = $"products/{command.OwnerUserId}/{Guid.NewGuid():N}/{image.UploadId:N}{Path.GetExtension(image.StorageKey)}";

                await _imageStorage.PromoteAsync(image.StorageKey, permanentKey, cancellationToken);

                image.StorageKey = permanentKey;
                image.RentableProduct = rentableProduct;
                image.SortOrder = sortOrder;
                image.IsPrimary = sortOrder == 0;
            }
        }

        return new CreateRentalListingResponse(rentableProduct.Title, (RentableProductStatus)rentableProduct.Status, rentableProduct.CreatedAt);
    }
}
