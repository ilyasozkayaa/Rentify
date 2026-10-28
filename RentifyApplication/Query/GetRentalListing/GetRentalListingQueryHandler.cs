using MediatR;
using RentifyApplication.IRepositories;
using RentifyApplication.IServices;
using RentifyDomain.Enum;

namespace RentifyApplication.Query.GetRentalListing;

public sealed class GetRentalListingQueryHandler : IRequestHandler<GetRentalListingQuery, RentalListingDetails?>
{
    private readonly IRentableProductRepository _repository;
    private readonly IImageStorage _imageStorage;

    public GetRentalListingQueryHandler(IRentableProductRepository repository, IImageStorage imageStorage)
    {
        _repository = repository;
        _imageStorage = imageStorage;
    }

    public async Task<RentalListingDetails?> Handle(GetRentalListingQuery request, CancellationToken cancellationToken)
    {
        if (request.Id <= 0)
            return null;

        var listing = await _repository.GetActiveWithOwnerAndImagesAsync(request.Id, cancellationToken);
        if (listing is null)
            return null;

        var images = new List<RentalImageResult>(listing.Images.Count);
        foreach (var image in listing.Images.OrderBy(image => image.SortOrder))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (image.StorageKey.StartsWith("temporary/", StringComparison.Ordinal))
                continue;

            var url = await _imageStorage.CreatePresignedDownloadAsync(image.StorageKey, cancellationToken);
            images.Add(new RentalImageResult(image.UploadId, url, image.SortOrder, image.IsPrimary));
        }

        return new RentalListingDetails(
            listing.Id,
            ((RentalType)listing.RentalType).ToString(),
            ((CityCode)listing.CityCode).ToString(),
            listing.District,
            listing.Title,
            listing.Description,
            listing.Price,
            listing.Currency,
            listing.Attributes?.RootElement.Clone(),
            listing.CreatedAt,
            listing.Owner.FirstName,
            images);
    }
}
