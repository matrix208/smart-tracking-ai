using Microsoft.AspNetCore.Mvc;
using Tracking.Application.Interfaces;

namespace Tracking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DevicesController : ControllerBase
{
    private readonly IDeviceService _service;

    public DevicesController(
        IDeviceService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var devices = await _service.GetAllAsync(
            cancellationToken);

        return Ok(devices);
    }

    [HttpGet("{imei}")]
    public async Task<IActionResult> GetDetails(
        string imei,
        CancellationToken cancellationToken)
    {
        var device = await _service.GetDetailsAsync(
            imei,
            cancellationToken);

        if (device is null)
            return NotFound();

        return Ok(device);
    }
}
