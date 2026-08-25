using Microsoft.AspNetCore.Mvc;
using Tracking.Application.DTOs;
using Tracking.Application.Interfaces;

namespace Tracking.API.Controllers;

[ApiController]
[Route("api/tasks")]
public sealed class TasksController : ControllerBase
{
    private readonly ITaskService _service;

    public TasksController(ITaskService service)
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
        var task = await _service.GetByIdAsync(
            id,
            cancellationToken);

        if (task is null)
        {
            return NotFound(new
            {
                message = "Task not found."
            });
        }

        return Ok(task);
    }

    [HttpGet("trip/{tripId:long}")]
    public async Task<IActionResult> GetByTrip(
        long tripId,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetByTripIdAsync(
                tripId,
                cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        TaskRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var task = await _service.CreateAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(Get),
                new { id = task.Id },
                task);
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
        TaskRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var task = await _service.UpdateAsync(
                id,
                request,
                cancellationToken);

            if (task is null)
            {
                return NotFound(new
                {
                    message = "Task not found."
                });
            }

            return Ok(task);
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
                    message = "Task not found."
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
            var task = await _service.StartAsync(
                id,
                cancellationToken);

            if (task is null)
            {
                return NotFound(new
                {
                    message = "Task not found."
                });
            }

            return Ok(task);
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
            var task = await _service.CompleteAsync(
                id,
                cancellationToken);

            if (task is null)
            {
                return NotFound(new
                {
                    message = "Task not found."
                });
            }

            return Ok(task);
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
            var task = await _service.CancelAsync(
                id,
                cancellationToken);

            if (task is null)
            {
                return NotFound(new
                {
                    message = "Task not found."
                });
            }

            return Ok(task);
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
