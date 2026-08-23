using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Tracking.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Administrator")]
public sealed class AdminController : ControllerBase
{
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            message = "Administrator access granted.",
            userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value,
            username = User.Identity?.Name,
            role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
        });
    }
}
