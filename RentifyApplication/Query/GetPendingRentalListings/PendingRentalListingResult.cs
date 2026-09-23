using System.Text.Json;

namespace RentifyApplication.Query.GetPendingRentalListings;

public sealed record PendingRentalListingResult(
    int Id,
    int OwnerUserId,
    string RentalType,
    int CityCode,
    string? District,
    string Title,
    string? Description,
    decimal Price,
    string Currency,
    JsonElement? Attributes,
    DateTime CreatedAt);
