namespace RentifyDomain.Entities;

public sealed class User
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<RentableProduct> RentableProducts { get; set; } = [];
    public ICollection<Rent> Rents { get; set; } = [];
}
