using MediatR;
using RentifyApplication.IRepositories;
using RentifyApplication.IServices;
using RentifyDomain.Enum;

namespace RentifyApplication.Query.GetMyRents;

public sealed class GetMyRentsQueryHandler : IRequestHandler<GetMyRentsQuery, MyRentsResponse>
{
    private readonly IRentRepository _rentRepository;
    private readonly IImageStorage _imageStorage;

    public GetMyRentsQueryHandler(IRentRepository rentRepository, IImageStorage imageStorage)
    {
        _rentRepository = rentRepository;
        _imageStorage = imageStorage;
    }

    public async Task<MyRentsResponse> Handle(GetMyRentsQuery request, CancellationToken cancellationToken)
    {
        var rents = await _rentRepository.GetByRenterAsync(
            request.RenterUserId,
            request.Page,
            request.PageSize,
            cancellationToken);
        var hasNextPage = rents.Count > request.PageSize;
        if (hasNextPage)
            rents.RemoveAt(rents.Count - 1);

        var results = new List<MyRentResult>(rents.Count);
        foreach (var rent in rents)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var coverUrl = rent.CoverImageStorageKey is null
                ? null
                : await _imageStorage.CreatePresignedDownloadAsync(rent.CoverImageStorageKey, cancellationToken);

            results.Add(new MyRentResult(
                rent.Id,
                rent.RentableProductId,
                rent.ListingTitle,
                ((RentalType)rent.RentalType).ToString(),
                ((CityCode)rent.CityCode).ToString(),
                rent.District,
                coverUrl,
                rent.StartDate,
                rent.EndDate,
                rent.DailyPrice,
                rent.TotalPrice,
                rent.Currency,
                (RentStatus)rent.Status,
                rent.CreatedAt));
        }

        return new MyRentsResponse(request.Page, request.PageSize, hasNextPage, results);
    }
}
