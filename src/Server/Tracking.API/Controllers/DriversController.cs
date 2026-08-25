using Microsoft.AspNetCore.Mvc;
using Tracking.Application.DTOs;
using Tracking.Application.Interfaces;

namespace Tracking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DriversController : ControllerBase
{
    private readonly IDriverService _service;

    public DriversController(IDriverService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var drivers = await _service.GetAllAsync(
            cancellationToken);

        return Ok(drivers);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(
        long id,
        CancellationToken cancellationToken)
    {
        var driver = await _service.GetByIdAsync(
            id,
            cancellationToken);

        if (driver is null)
        {
            return NotFound(new
            {
                message = "Driver not found."
            });
        }

        return Ok(driver);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        DriverRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var driver = await _service.CreateAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(Get),
                new { id = driver.Id },
                driver);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(
        long id,
        DriverRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var driver = await _service.UpdateAsync(
                id,
                request,
                cancellationToken);

            if (driver is null)
            {
                return NotFound(new
                {
                    message = "Driver not found."
                });
            }

            return Ok(driver);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(
        long id,
        CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(
            id,
            cancellationToken);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Driver not found."
            });
        }

        return NoContent();
    }
}
