using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tracking.API.DTOs;
using Tracking.Security.Password;
using Tracking.Storage.Data;
using Tracking.Storage.Entities;

namespace Tracking.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Administrator")]
public sealed class UsersController : ControllerBase
{
    private readonly TrackingDbContext _db;
    private readonly PasswordHasher _passwordHasher;

    public UsersController(TrackingDbContext db, PasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _db.Users
            .OrderBy(u => u.Username)
            .Select(u => new UserSummaryDto(
                u.Id,
                u.Username,
                u.DisplayName,
                u.IsActive,
                u.UserRoles.Select(ur => ur.Role.Name).ToList()))
            .ToListAsync(cancellationToken);

        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username and password are required." });
        }

        var exists = await _db.Users.AnyAsync(u => u.Username == request.Username, cancellationToken);

        if (exists)
        {
            return Conflict(new { message = "Username already exists." });
        }

        var user = new UserEntity
        {
            Username = request.Username.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName)
                ? request.Username.Trim()
                : request.DisplayName.Trim(),
            Role = "Viewer",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        if (request.RoleIds is { Count: > 0 })
        {
            await SetUserRolesAsync(user.Id, request.RoleIds, cancellationToken);
        }

        return CreatedAtAction(nameof(GetAll), new { id = user.Id });
    }

    [HttpPut("{id:long}/roles")]
    public async Task<IActionResult> SetRoles(
        long id,
        SetUserRolesRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _db.Users.FindAsync([id], cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        await SetUserRolesAsync(id, request.RoleIds, cancellationToken);

        var roleNames = await _db.UserRoles
            .Where(ur => ur.UserId == id)
            .Select(ur => ur.Role.Name)
            .ToListAsync(cancellationToken);

        // Keep the legacy single-role field roughly in sync for any
        // remaining code paths that still read it directly.
        user.Role = roleNames.Contains("Administrator")
            ? "Administrator"
            : roleNames.FirstOrDefault() ?? user.Role;

        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPut("{id:long}/status")]
    public async Task<IActionResult> SetActive(
        long id,
        SetUserActiveRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _db.Users.FindAsync([id], cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        user.IsActive = request.IsActive;
        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task SetUserRolesAsync(
        long userId,
        IReadOnlyCollection<long> roleIds,
        CancellationToken cancellationToken)
    {
        var existing = await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .ToListAsync(cancellationToken);

        _db.UserRoles.RemoveRange(existing);

        foreach (var roleId in roleIds.Distinct())
        {
            _db.UserRoles.Add(new UserRoleEntity
            {
                UserId = userId,
                RoleId = roleId
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
