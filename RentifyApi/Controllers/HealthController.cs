using MediatR;
using Microsoft.AspNetCore.Mvc;
using RentifyApplication.Query;

namespace RentifyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ISender _sender;

    public HealthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetHealthStatusQuery(),
            cancellationToken);

        return Ok(result);
    }
}
