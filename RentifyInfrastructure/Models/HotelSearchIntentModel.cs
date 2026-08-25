namespace RentifyInfrastructure.Models;

public sealed class HotelSearchIntentModel
{
    public int? MinimumStars { get; set; }
    public bool? BreakfastIncluded { get; set; }
    public bool? Pool { get; set; }
    public string? RoomType { get; set; }
}
