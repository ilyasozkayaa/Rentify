namespace RentifyInfrastructure.Models;

public sealed class PropertySearchIntentModel
{
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public bool? SeaView { get; set; }
    public bool? Detached { get; set; }
    public bool? Furnished { get; set; }
    public bool? Pool { get; set; }
    public bool? Garden { get; set; }
}
