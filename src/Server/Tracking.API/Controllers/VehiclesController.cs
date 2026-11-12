using Microsoft.AspNetCore.Mvc;
using Tracking.Application.DTOs;
using Tracking.Application.Interfaces;

namespace Tracking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class VehiclesController : ControllerBase
{
    private readonly IVehicleService _service;

    public VehiclesController(IVehicleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var vehicles = await _service.GetAllAsync(
            cancellationToken);

        return Ok(vehicles);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(
        long id,
        CancellationToken cancellationToken)
    {
        var vehicle = await _service.GetByIdAsync(
            id,
            cancellationToken);

        if (vehicle is null)
        {
            return NotFound(new
            {
                message = "Vehicle not found."
            });
        }

        return Ok(vehicle);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        VehicleRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var vehicle = await _service.CreateAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(Get),
                new { id = vehicle.Id },
                vehicle);
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
        VehicleRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var vehicle = await _service.UpdateAsync(
                id,
                request,
                cancellationToken);

            if (vehicle is null)
            {
                return NotFound(new
                {
                    message = "Vehicle not found."
                });
            }

            return Ok(vehicle);
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
                message = "Vehicle not found."
            });
        }

        return NoContent();
    }
}
