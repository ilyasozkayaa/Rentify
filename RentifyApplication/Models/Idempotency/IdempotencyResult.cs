namespace RentifyApplication.Models.Idempotency;

public sealed record IdempotencyResult(bool Acquired, bool IsProcessing, string? Response, int? StatusCode, string? ContentType);