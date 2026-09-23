using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentifyApi.Models;
using RentifyApplication.Command.CreateRentalListing;
using RentifyApplication.Command.ActivateRentalListings;

namespace RentifyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class RentalListingsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public RentalListingsController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRentalListingRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!int.TryParse(userIdClaim, out var ownerUserId) || ownerUserId <= 0)
            return Unauthorized();

        var command = _mapper.Map<CreateRentalListingCommand>(request) with { OwnerUserId = ownerUserId };

        var response = await _sender.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut("activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ActivateListings([FromBody] ActivateRentalListingsCommand command, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(command, cancellationToken);

        return Ok(response);
    }
}
