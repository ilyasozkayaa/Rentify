using Microsoft.AspNetCore.Mvc;

namespace RentifyApi.Idempotency;

[AttributeUsage(AttributeTargets.Method)]
public sealed class IdempotentAttribute : TypeFilterAttribute
{
    public IdempotentAttribute() : base(typeof(IdempotencyFilter))
    {
    }
}