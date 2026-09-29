namespace RentifyApplication.Query;

public sealed record RentalImageResult(Guid Id, string Url, int SortOrder, bool IsPrimary);