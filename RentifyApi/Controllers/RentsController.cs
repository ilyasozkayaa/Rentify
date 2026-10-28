using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentifyApi.Idempotency;
using RentifyApi.Models;
using RentifyApplication.Command.CreateRent;
using RentifyApplication.Query.GetMyRents;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RentifyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class RentsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public RentsController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    [HttpPost]
    [Idempotent]
    public async Task<IActionResult> Create([FromBody] CreateRentRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!int.TryParse(userIdClaim, out var renterUserId) || renterUserId <= 0)
            return Unauthorized();

        var command = _mapper.Map<CreateRentCommand>(request) with { RenterUserId = renterUserId };
        var response = await _sender.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetMine([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!int.TryParse(userIdClaim, out var renterUserId) || renterUserId <= 0)
            return Unauthorized();

        if (page < 1 || page > 1_000_000 || pageSize < 1)
            return BadRequest("Page must be between 1 and 1,000,000 and pageSize must be positive.");

        var boundedPageSize = Math.Min(pageSize, 50);
        var response = await _sender.Send(new GetMyRentsQuery(renterUserId, page, boundedPageSize), cancellationToken);
        return Ok(response);
    }
}
