namespace RentifyInfrastructure.Models;

public sealed class PropertySearchIntentModel
{
    public string? PropertyType { get; set; }
    public bool? SeaView { get; set; }
    public bool? Detached { get; set; }
    public int? Bedrooms { get; set; }
    public bool? Pool { get; set; }
}
