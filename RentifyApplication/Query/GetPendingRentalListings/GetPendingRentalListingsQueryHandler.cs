using MediatR;
using RentifyApplication.IRepositories;
using RentifyApplication.IServices;
using RentifyApplication.Query;
using RentifyDomain.Entities;
using RentifyDomain.Enum;

namespace RentifyApplication.Query.GetPendingRentalListings;

public sealed class GetPendingRentalListingsQueryHandler : IRequestHandler<GetPendingRentalListingsQuery, GetPendingRentalListingsResponse>
{
    private readonly IRentableProductRepository _rentableProductRepository;
    private readonly IImageStorage _imageStorage;

    public GetPendingRentalListingsQueryHandler(IRentableProductRepository rentableProductRepository, IImageStorage imageStorage)
    {
        _rentableProductRepository = rentableProductRepository;
        _imageStorage = imageStorage;
    }

    public async Task<GetPendingRentalListingsResponse> Handle(
    GetPendingRentalListingsQuery request,
    CancellationToken cancellationToken)
    {
        var listings = await _rentableProductRepository.GetPendingAsync(request.Page, request.PageSize, cancellationToken);

        var hasNextPage = listings.Count > request.PageSize;

        var results = new List<PendingRentalListingResult>(request.PageSize);

        foreach (var listing in listings.Take(request.PageSize))
        {
            cancellationToken.ThrowIfCancellationRequested();
            results.Add(await MapAsync(listing, cancellationToken));
        }

        return new GetPendingRentalListingsResponse(results, hasNextPage);
    }

    private async Task<PendingRentalListingResult> MapAsync(RentableProduct listing, CancellationToken cancellationToken)
    {
        var images = new List<RentalImageResult>(listing.Images.Count);

        foreach (var image in listing.Images.OrderBy(image => image.SortOrder))
        {
            var url = await _imageStorage.CreatePresignedDownloadAsync(image.StorageKey, cancellationToken);

            images.Add(new RentalImageResult(image.UploadId, url, image.SortOrder, image.IsPrimary));
        }

        return new PendingRentalListingResult(
            listing.Id,
            listing.OwnerUserId,
            ((RentalType)listing.RentalType).ToString(),
            listing.CityCode,
            listing.District,
            listing.Title,
            listing.Description,
            listing.Price,
            listing.Currency,
            listing.Attributes?.RootElement.Clone(),
            listing.CreatedAt,
            images);
    }
}