namespace RentifyDomain.Entities;

public sealed class Rent
{
    public int Id { get; set; }
    public int RenterUserId { get; set; }
    public int RentableProductId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User RenterUser { get; set; } = null!;
    public RentableProduct RentableProduct { get; set; } = null!;
}
