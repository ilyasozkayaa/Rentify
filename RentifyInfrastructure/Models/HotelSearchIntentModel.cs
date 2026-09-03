namespace RentifyInfrastructure.Models;

public sealed class HotelSearchIntentModel
{
    public int? Stars { get; set; }
    public bool? BreakfastIncluded { get; set; }
    public bool? Pool { get; set; }
    public bool? OpenBuffet { get; set; }
    public bool? AllInclusive { get; set; }
    public bool? Restaurant { get; set; }
    public bool? Gym { get; set; }
    public bool? Spa { get; set; }
    public int? GuestCapacity { get; set; }
    public bool? Parking { get; set; }
    public bool? SeaView { get; set; }
    public bool? AirConditioning { get; set; }
    public bool? Wifi { get; set; }
}
