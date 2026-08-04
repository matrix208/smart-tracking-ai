using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tracking.Storage.Data;

namespace Tracking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PositionsController : ControllerBase
{
    private readonly TrackingDbContext _context;

    public PositionsController(
        TrackingDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var positions = await _context.Positions
            .AsNoTracking()
            .OrderByDescending(x => x.ServerTime)
            .Take(100)
            .ToListAsync(cancellationToken);

        return Ok(positions);
    }
}