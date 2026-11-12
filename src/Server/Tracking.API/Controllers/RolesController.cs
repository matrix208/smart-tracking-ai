using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tracking.API.DTOs;
using Tracking.Storage.Data;
using Tracking.Storage.Entities;

namespace Tracking.API.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Roles = "Administrator")]
public sealed class RolesController : ControllerBase
{
    private readonly TrackingDbContext _db;

    public RolesController(TrackingDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var roles = await _db.Roles
            .OrderBy(r => r.Name)
            .Select(r => new RoleSummaryDto(
                r.Id,
                r.Name,
                r.Description,
                r.IsSystemRole,
                r.RolePermissions.Count,
                r.UserRoles.Count))
            .ToListAsync(cancellationToken);

        return Ok(roles);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var role = await _db.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (role is null)
        {
            return NotFound();
        }

        return Ok(new RoleDetailDto(
            role.Id,
            role.Name,
            role.Description,
            role.IsSystemRole,
            role.RolePermissions.Select(rp => rp.Permission.Key).ToList()));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Role name is required." });
        }

        var exists = await _db.Roles.AnyAsync(r => r.Name == request.Name, cancellationToken);

        if (exists)
        {
            return Conflict(new { message = "A role with this name already exists." });
        }

        var role = new RoleEntity
        {
            Name = request.Name.Trim(),
            Description = request.Description ?? string.Empty,
            IsSystemRole = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync(cancellationToken);

        if (request.PermissionKeys is { Count: > 0 })
        {
            await SetRolePermissionsAsync(role.Id, request.PermissionKeys, cancellationToken);
        }

        return CreatedAtAction(nameof(GetById), new { id = role.Id }, new { id = role.Id });
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var role = await _db.Roles.FindAsync([id], cancellationToken);

        if (role is null)
        {
            return NotFound();
        }

        if (role.IsSystemRole && !string.IsNullOrWhiteSpace(request.Name) && request.Name != role.Name)
        {
            return BadRequest(new { message = "System role names cannot be changed." });
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            role.Name = request.Name.Trim();
        }

        if (request.Description is not null)
        {
            role.Description = request.Description;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var role = await _db.Roles.FindAsync([id], cancellationToken);

        if (role is null)
        {
            return NotFound();
        }

        if (role.IsSystemRole)
        {
            return BadRequest(new { message = "System roles cannot be deleted." });
        }

        _db.Roles.Remove(role);
        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPut("{id:long}/permissions")]
    public async Task<IActionResult> SetPermissions(
        long id,
        SetRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var role = await _db.Roles.FindAsync([id], cancellationToken);

        if (role is null)
        {
            return NotFound();
        }

        await SetRolePermissionsAsync(id, request.PermissionKeys, cancellationToken);

        return NoContent();
    }

    private async Task SetRolePermissionsAsync(
        long roleId,
        IReadOnlyCollection<string> permissionKeys,
        CancellationToken cancellationToken)
    {
        var existing = await _db.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);

        _db.RolePermissions.RemoveRange(existing);

        var permissions = await _db.Permissions
            .Where(p => permissionKeys.Contains(p.Key))
            .ToListAsync(cancellationToken);

        foreach (var permission in permissions)
        {
            _db.RolePermissions.Add(new RolePermissionEntity
            {
                RoleId = roleId,
                PermissionId = permission.Id
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
