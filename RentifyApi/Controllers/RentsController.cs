using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentifyApi.Models;
using RentifyApplication.Command.CreateRent;

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
    public async Task<IActionResult> Create([FromBody] CreateRentRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!int.TryParse(userIdClaim, out var renterUserId) || renterUserId <= 0)
            return Unauthorized();

        var command = _mapper.Map<CreateRentCommand>(request) with { RenterUserId = renterUserId };
        var response = await _sender.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, response);
    }
}