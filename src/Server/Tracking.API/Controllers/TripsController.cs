using Microsoft.AspNetCore.Mvc;
using Tracking.Application.DTOs;
using Tracking.Application.Interfaces;

namespace Tracking.API.Controllers;

[ApiController]
[Route("api/trips")]
public sealed class TripsController : ControllerBase
{
    private readonly ITripService _service;

    public TripsController(ITripService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetAllAsync(
                cancellationToken));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(
        long id,
        CancellationToken cancellationToken)
    {
        var trip = await _service.GetByIdAsync(
            id,
            cancellationToken);

        if (trip is null)
        {
            return NotFound(new
            {
                message = "Trip not found."
            });
        }

        return Ok(trip);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        TripRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var trip = await _service.CreateAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(Get),
                new { id = trip.Id },
                trip);
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
        TripRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var trip = await _service.UpdateAsync(
                id,
                request,
                cancellationToken);

            if (trip is null)
            {
                return NotFound(new
                {
                    message = "Trip not found."
                });
            }

            return Ok(trip);
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
        try
        {
            var deleted = await _service.DeleteAsync(
                id,
                cancellationToken);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Trip not found."
                });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("{id:long}/start")]
    public async Task<IActionResult> Start(
        long id,
        CancellationToken cancellationToken)
    {
        try
        {
            var trip = await _service.StartAsync(
                id,
                cancellationToken);

            if (trip is null)
            {
                return NotFound(new
                {
                    message = "Trip not found."
                });
            }

            return Ok(trip);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("{id:long}/complete")]
    public async Task<IActionResult> Complete(
        long id,
        CancellationToken cancellationToken)
    {
        try
        {
            var trip = await _service.CompleteAsync(
                id,
                cancellationToken);

            if (trip is null)
            {
                return NotFound(new
                {
                    message = "Trip not found."
                });
            }

            return Ok(trip);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("{id:long}/cancel")]
    public async Task<IActionResult> Cancel(
        long id,
        CancellationToken cancellationToken)
    {
        try
        {
            var trip = await _service.CancelAsync(
                id,
                cancellationToken);

            if (trip is null)
            {
                return NotFound(new
                {
                    message = "Trip not found."
                });
            }

            return Ok(trip);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }
}
