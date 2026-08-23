using Microsoft.AspNetCore.Mvc;
using Tracking.Application.Interfaces;

namespace Tracking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DeviceStatesController : ControllerBase
{
    private readonly IDeviceStateService _service;

    public DeviceStatesController(
        IDeviceStateService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var states = await _service.GetAllAsync(
            cancellationToken);

        return Ok(states);
    }

    [HttpGet("{deviceId}")]
    public async Task<IActionResult> Get(
        string deviceId,
        CancellationToken cancellationToken)
    {
        var state = await _service.GetByDeviceIdAsync(
            deviceId,
            cancellationToken);

        if (state is null)
            return NotFound();

        return Ok(state);
    }
}