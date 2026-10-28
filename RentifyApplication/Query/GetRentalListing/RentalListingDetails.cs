using System.Text.Json;

namespace RentifyApplication.Query.GetRentalListing;

public sealed record RentalListingDetails(
    int Id,
    string RentalType,
    string City,
    string? District,
    string Title,
    string? Description,
    decimal Price,
    string Currency,
    JsonElement? Attributes,
    DateTime CreatedAt,
    string OwnerFirstName,
    IReadOnlyCollection<RentalImageResult> Images);
