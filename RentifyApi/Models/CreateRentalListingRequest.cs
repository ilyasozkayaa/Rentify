using System.Text.Json;

namespace RentifyApi.Models;

public sealed record CreateRentalListingRequest(
    int RentalType,
    int CityCode,
    string? District,
    string Title,
    string? Description,
    decimal Price,
    string Currency,
    JsonElement Attributes);
