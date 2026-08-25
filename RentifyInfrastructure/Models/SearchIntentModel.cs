using System.Text.Json.Serialization;

namespace RentifyInfrastructure.Models;

public sealed class SearchIntentModel
{
    [JsonPropertyName("rentalType")]
    public string RentalType { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("startDate")]
    public string? StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public string? EndDate { get; set; }

    [JsonPropertyName("searchText")]
    public string? SearchText { get; set; }

    [JsonPropertyName("minPrice")]
    public decimal? MinPrice { get; set; }

    [JsonPropertyName("maxPrice")]
    public decimal? MaxPrice { get; set; }

    [JsonPropertyName("vehicleCriteria")]
    public VehicleSearchIntentModel? VehicleCriteria { get; set; }

    [JsonPropertyName("propertyCriteria")]
    public PropertySearchIntentModel? PropertyCriteria { get; set; }

    [JsonPropertyName("hotelCriteria")]
    public HotelSearchIntentModel? HotelCriteria { get; set; }
}
