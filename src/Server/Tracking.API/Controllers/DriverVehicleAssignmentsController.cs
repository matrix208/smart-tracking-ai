using Microsoft.AspNetCore.Mvc;
using Tracking.Application.DTOs;
using Tracking.Application.Interfaces;

namespace Tracking.API.Controllers;

[ApiController]
[Route("api/driver-vehicle-assignments")]
public sealed class DriverVehicleAssignmentsController : ControllerBase
{
    private readonly IDriverVehicleAssignmentService _service;

    public DriverVehicleAssignmentsController(
        IDriverVehicleAssignmentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(
        long id,
        CancellationToken cancellationToken)
    {
        var assignment = await _service.GetByIdAsync(
            id,
            cancellationToken);

        if (assignment is null)
        {
            return NotFound(new
            {
                message = "Assignment not found."
            });
        }

        return Ok(assignment);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        DriverVehicleAssignmentRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var assignment =
                await _service.CreateAsync(
                    request,
                    cancellationToken);

            return CreatedAtAction(
                nameof(Get),
                new { id = assignment.Id },
                assignment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(
        long id,
        DriverVehicleAssignmentRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var assignment =
                await _service.UpdateAsync(
                    id,
                    request,
                    cancellationToken);

            if (assignment is null)
            {
                return NotFound(new
                {
                    message = "Assignment not found."
                });
            }

            return Ok(assignment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
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
                message = "Assignment not found."
            });
        }

        return NoContent();
    }

    [HttpPost("assign")]
    public async Task<IActionResult> Assign(
        AssignDriverVehicleRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var assignment =
                await _service.AssignAsync(
                    request,
                    cancellationToken);

            return CreatedAtAction(
                nameof(Get),
                new { id = assignment.Id },
                assignment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("{id:long}/unassign")]
    public async Task<IActionResult> Unassign(
        long id,
        CancellationToken cancellationToken)
    {
        var assignment =
            await _service.UnassignAsync(
                id,
                cancellationToken);

        if (assignment is null)
        {
            return NotFound(new
            {
                message = "Assignment not found."
            });
        }

        return Ok(assignment);
    }

    [HttpGet("vehicle/{vehicleId:long}/driver")]
    public async Task<IActionResult> GetActiveDriverForVehicle(
        long vehicleId,
        CancellationToken cancellationToken)
    {
        var assignment =
            await _service.GetActiveDriverForVehicleAsync(
                vehicleId,
                cancellationToken);

        if (assignment is null)
        {
            return NotFound(new
            {
                message = "No active driver assigned to this vehicle."
            });
        }

        return Ok(assignment);
    }

    [HttpGet("driver/{driverId:long}/vehicle")]
    public async Task<IActionResult> GetActiveVehicleForDriver(
        long driverId,
        CancellationToken cancellationToken)
    {
        var assignment =
            await _service.GetActiveVehicleForDriverAsync(
                driverId,
                cancellationToken);

        if (assignment is null)
        {
            return NotFound(new
            {
                message = "No active vehicle assigned to this driver."
            });
        }

        return Ok(assignment);
    }

    [HttpGet("vehicle/{vehicleId:long}")]
    public async Task<IActionResult> GetVehicleAssignments(
        long vehicleId,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetVehicleAssignmentsAsync(
                vehicleId,
                cancellationToken));
    }

    [HttpGet("driver/{driverId:long}")]
    public async Task<IActionResult> GetDriverAssignments(
        long driverId,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetDriverAssignmentsAsync(
                driverId,
                cancellationToken));
    }
}
