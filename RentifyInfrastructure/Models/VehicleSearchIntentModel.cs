namespace RentifyInfrastructure.Models;

public sealed class VehicleSearchIntentModel
{
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int? ModelYear { get; set; }
    public string? Transmission { get; set; }
    public string? FuelType { get; set; }
    public int? Seats { get; set; }
}
