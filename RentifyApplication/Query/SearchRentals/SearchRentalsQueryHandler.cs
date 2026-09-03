using MediatR;
using RentifyApplication.Exceptions;
using RentifyApplication.Exceptions.Enums;
using RentifyApplication.IRepositories;
using RentifyApplication.IServices;
using RentifyApplication.Query.SearchRentals.SearchCriteria;
using RentifyDomain.Enum;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RentifyApplication.Query.SearchRentals;

public sealed class SearchRentalsQueryHandler : IRequestHandler<SearchRentalsQuery, SearchRentalsResponse>
{
    private readonly ISearchIntentService _searchIntentService;
    private readonly IRentableProductRepository _rentableProductRepository;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

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

        if (products.Count == 0)
            return new SearchRentalsResponse([]);

        switch (searchIntent.RentalType)
        {
            case RentalType.Vehicle:
                var vehicleCriteria = searchIntent.VehicleCriteria;

                if (vehicleCriteria is not null)
                {
                    products = products.Where(x =>
                    {
                        var attributes = x.Attributes?.Deserialize<VehicleSearchCriteria>(JsonOptions);

                        if (attributes is null)
                            return false;

                        return
                            (vehicleCriteria.Brand is null || string.Equals(vehicleCriteria.Brand, attributes.Brand, StringComparison.OrdinalIgnoreCase)) &&
                            (vehicleCriteria.Model is null || string.Equals(vehicleCriteria.Model, attributes.Model, StringComparison.OrdinalIgnoreCase)) &&
                            (vehicleCriteria.ModelYear is null || vehicleCriteria.ModelYear == attributes.ModelYear) &&
                            (vehicleCriteria.Transmission is null || vehicleCriteria.Transmission == attributes.Transmission) &&
                            (vehicleCriteria.FuelType is null || vehicleCriteria.FuelType == attributes.FuelType) &&
                            (vehicleCriteria.Seats is null || vehicleCriteria.Seats <= attributes.Seats);
                    }).ToList();
                }

                break;

            case RentalType.Property:
            case RentalType.Villa:

                var propertyCriteria = searchIntent.PropertyCriteria;

                if (propertyCriteria is not null)
                {
                    products = products.Where(x =>
                    {
                        var attributes = x.Attributes?.Deserialize<PropertySearchCriteria>(JsonOptions);

                        if (attributes is null)
                            return false;

                        return
                            (propertyCriteria.Bedrooms is null || propertyCriteria.Bedrooms == attributes.Bedrooms) &&
                            (propertyCriteria.Bathrooms is null || propertyCriteria.Bathrooms == attributes.Bathrooms) &&
                            (propertyCriteria.SeaView is null || propertyCriteria.SeaView == attributes.SeaView) &&
                            (propertyCriteria.Detached is null || propertyCriteria.Detached == attributes.Detached) &&
                            (propertyCriteria.Furnished is null || propertyCriteria.Furnished == attributes.Furnished) &&
                            (propertyCriteria.Pool is null || propertyCriteria.Pool == attributes.Pool) &&
                            (propertyCriteria.Garden is null || propertyCriteria.Garden == attributes.Garden);
                    }).ToList();
                }

                break;

            case RentalType.Hotel:

                var hotelCriteria = searchIntent.HotelCriteria;

                if (hotelCriteria is not null)
                {
                    products = products.Where(x =>
                    {
                        var attributes = x.Attributes?.Deserialize<HotelSearchCriteria>(JsonOptions);

                        if (attributes is null)
                            return false;

                        return
                            (hotelCriteria.Stars is null || hotelCriteria.Stars <= attributes.Stars) &&
                            (hotelCriteria.BreakfastIncluded is null || hotelCriteria.BreakfastIncluded == attributes.BreakfastIncluded) &&
                            (hotelCriteria.Pool is null || hotelCriteria.Pool == attributes.Pool) &&
                            (hotelCriteria.OpenBuffet is null || hotelCriteria.OpenBuffet == attributes.OpenBuffet) &&
                            (hotelCriteria.AllInclusive is null || hotelCriteria.AllInclusive == attributes.AllInclusive) &&
                            (hotelCriteria.Restaurant is null || hotelCriteria.Restaurant == attributes.Restaurant) &&
                            (hotelCriteria.Gym is null || hotelCriteria.Gym == attributes.Gym) &&
                            (hotelCriteria.Spa is null || hotelCriteria.Spa == attributes.Spa) &&
                            (hotelCriteria.GuestCapacity is null || hotelCriteria.GuestCapacity <= attributes.GuestCapacity) &&
                            (hotelCriteria.Parking is null || hotelCriteria.Parking == attributes.Parking) &&
                            (hotelCriteria.SeaView is null || hotelCriteria.SeaView == attributes.SeaView) &&
                            (hotelCriteria.AirConditioning is null || hotelCriteria.AirConditioning == attributes.AirConditioning) &&
                            (hotelCriteria.Wifi is null || hotelCriteria.Wifi == attributes.Wifi);
                    }).ToList();
                }

                break;
        }

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
