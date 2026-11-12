using Microsoft.AspNetCore.Mvc;
using Tracking.Application.Interfaces;

namespace Tracking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PositionsController : ControllerBase
{
    private readonly IPositionService _positionService;

    public PositionsController(IPositionService positionService)
    {
        _positionService = positionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var positions = await _positionService.GetLatestAsync(
            100,
            cancellationToken);

        return Ok(positions);
    }
}