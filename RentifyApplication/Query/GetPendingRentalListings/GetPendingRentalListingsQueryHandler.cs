using MediatR;
using RentifyApplication.IRepositories;
using RentifyDomain.Enum;

namespace RentifyApplication.Query.GetPendingRentalListings;

public sealed class GetPendingRentalListingsQueryHandler : IRequestHandler<GetPendingRentalListingsQuery, GetPendingRentalListingsResponse>
{
    private readonly IRentableProductRepository _rentableProductRepository;

    public GetPendingRentalListingsQueryHandler(IRentableProductRepository rentableProductRepository)
    {
        _rentableProductRepository = rentableProductRepository;
    }

    public async Task<GetPendingRentalListingsResponse> Handle(GetPendingRentalListingsQuery request, CancellationToken cancellationToken)
    {
        var listings = await _rentableProductRepository.GetPendingAsync(request.Page, request.PageSize, cancellationToken);
        var hasNextPage = listings.Count > request.PageSize;

        var results = listings.Take(request.PageSize).Select(listing => new PendingRentalListingResult(
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
            listing.CreatedAt)).ToArray();

        return new GetPendingRentalListingsResponse(results, hasNextPage);
    }
}
