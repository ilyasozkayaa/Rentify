using MediatR;
using RentifyApplication.Exceptions;
using RentifyApplication.Exceptions.Enums;
using RentifyApplication.IRepositories;
using RentifyApplication.IServices;
using RentifyDomain.Enum;

namespace RentifyApplication.Query.SearchRentals;

public sealed class SearchRentalsQueryHandler : IRequestHandler<SearchRentalsQuery, SearchRentalsResponse>
{
    private readonly ISearchIntentService _searchIntentService;
    private readonly IRentableProductRepository _rentableProductRepository;

    public SearchRentalsQueryHandler(ISearchIntentService searchIntentService, IRentableProductRepository rentableProductRepository)
    {
        _searchIntentService = searchIntentService;
        _rentableProductRepository = rentableProductRepository;
    }

    public async Task<SearchRentalsResponse> Handle(SearchRentalsQuery request, CancellationToken cancellationToken)
    {
        var searchIntent = await _searchIntentService.CreateIntentAsync(request.Query, cancellationToken);

        var missingCriteria = new List<string>();

        if (searchIntent.RentalType == RentalType.Unknown)
            missingCriteria.Add("rental type");

        if (searchIntent.CityCode == 0)
            missingCriteria.Add("city");

        if (missingCriteria.Count > 0)
            throw new BusinessException(
                $"Please specify: {string.Join(", ", missingCriteria)}.",
                BusinessErrorCode.SearchCriteriaRequired);

        var products = await _rentableProductRepository.SearchAsync(searchIntent, cancellationToken);

        return new SearchRentalsResponse(products.Select(x => new RentalSearchResult(
            x.Id,
            ((RentalType)x.RentalType).ToString(),
            x.Title,
            ((CityCode)x.CityCode).ToString(),
            x.Price,
            x.Currency,
            x.Description)).ToArray());
    }
}
