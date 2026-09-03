using RentifyDomain.Enum;

namespace RentifyInfrastructure.Models;

public sealed class VehicleSearchIntentModel
{
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int? ModelYear { get; set; }
    public TransmissionType? Transmission { get; set; }
    public FuelType? FuelType { get; set; }
    public int? Seats { get; set; }
}
