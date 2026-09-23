using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RentifyApplication.Query.SearchRentals;
using RentifyApplication.Query.GetPendingRentalListings;

namespace RentifyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly ISender _sender;

    public SearchController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("search")]
    [EnableRateLimiting("llm-search")]
    public async Task<IActionResult> Search([FromBody] SearchRentalsQuery query, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingListings([FromQuery] GetPendingRentalListingsQuery query, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(query, cancellationToken);

        return Ok(result);
    }
}

