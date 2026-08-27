using RentifyDomain.Enum;
using System.Text.Json;

namespace RentifyDomain.Entities;

public sealed class RentableProduct
{
    public int Id { get; set; }
    public int OwnerUserId { get; set; }
    public int RentalType { get; set; }
    public int CityCode { get; set; }
    public string? District { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "TRY";
    public JsonDocument? Attributes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User Owner { get; set; } = null!;
    public ICollection<Rent> Rents { get; set; } = [];
}
