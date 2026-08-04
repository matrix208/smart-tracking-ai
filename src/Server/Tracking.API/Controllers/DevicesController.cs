using Microsoft.AspNetCore.Mvc;
using Tracking.Storage.Repositories;

namespace Tracking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceRepository _deviceRepository;

    public DevicesController(
        IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var devices = await _deviceRepository
            .GetAllAsync(cancellationToken);

        return Ok(devices);
    }
}