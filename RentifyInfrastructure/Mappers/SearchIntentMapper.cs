using RentifyApplication.Query.SearchRentals.SearchCriteria;
using RentifyDomain.Enum;
using RentifyInfrastructure.Models;

namespace RentifyInfrastructure.Mappers;

public static class SearchIntentMapper
{
    public static SearchIntent Map(SearchIntentModel model)
    {
        return new SearchIntent(
            RentalType: ParseRentalType(model.RentalType),
            CityCode: model.CityCode,
            StartDate: ParseDate(model.StartDate),
            EndDate: ParseDate(model.EndDate),
            SearchText: model.SearchText,
            MinPrice: model.MinPrice,
            MaxPrice: model.MaxPrice,
            Currency: model.Currency ?? Currency.TRY.ToString(),
            VehicleCriteria: MapVehicle(model.VehicleCriteria),
            PropertyCriteria: MapProperty(model.PropertyCriteria),
            HotelCriteria: MapHotel(model.HotelCriteria)
        );
    }

    private static RentalType ParseRentalType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return RentalType.Unknown;
        }

        return Enum.TryParse<RentalType>(value, ignoreCase: true, out var rentalType) ? rentalType : RentalType.Unknown;
    }

    private static DateOnly? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateOnly.TryParseExact(value, "yyyy-MM-dd", out var date) ? date : null;
    }

    private static VehicleSearchCriteria? MapVehicle(VehicleSearchIntentModel? model)
    {
        if (model is null)
        {
            return null;
        }

        return new VehicleSearchCriteria(
            Brand: model.Brand,
            Model: model.Model,
            ModelYear: model.ModelYear,
            Transmission: model.Transmission,
            FuelType: model.FuelType,
            Seats: model.Seats);
    }

    private static PropertySearchCriteria? MapProperty(PropertySearchIntentModel? model)
    {
        if (model is null)
        {
            return null;
        }

        return new PropertySearchCriteria(
            Bedrooms: model.Bedrooms,
            Bathrooms: model.Bathrooms,
            SeaView: model.SeaView,
            Detached: model.Detached,
            Furnished: model.Furnished,
            Pool: model.Pool,
            Garden: model.Garden);
    }

    private static HotelSearchCriteria? MapHotel(HotelSearchIntentModel? model)
    {
        if (model is null)
        {
            return null;
        }

        return new HotelSearchCriteria(
            Stars: model.Stars,
            BreakfastIncluded: model.BreakfastIncluded,
            OpenBuffet: model.OpenBuffet,
            AllInclusive: model.AllInclusive,
            Restaurant: model.Restaurant,
            Gym: model.Gym,
            Spa: model.Spa,
            Pool: model.Pool,
            GuestCapacity: model.GuestCapacity,
            Parking: model.Parking,
            SeaView: model.SeaView,
            AirConditioning: model.AirConditioning,
            Wifi: model.Wifi);
    }
}