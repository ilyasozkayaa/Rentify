using System.Text.Json;
using RentifyApplication.Command;

namespace RentifyApplication.Command.CreateRentalListing;

public sealed record CreateRentalListingCommand(
    int OwnerUserId,
    int RentalType,
    int CityCode,
    string? District,
    string Title,
    string? Description,
    decimal Price,
    string Currency,
    JsonElement Attributes) : ICommand<CreateRentalListingResponse>;
