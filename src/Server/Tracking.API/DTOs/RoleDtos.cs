namespace Tracking.API.DTOs;

public sealed record RoleSummaryDto(
    long Id,
    string Name,
    string Description,
    bool IsSystemRole,
    int PermissionCount,
    int UserCount);

public sealed record RoleDetailDto(
    long Id,
    string Name,
    string Description,
    bool IsSystemRole,
    List<string> PermissionKeys);

public sealed record CreateRoleRequest(
    string Name,
    string? Description,
    List<string>? PermissionKeys);

public sealed record UpdateRoleRequest(
    string? Name,
    string? Description);

public sealed record SetRolePermissionsRequest(
    List<string> PermissionKeys);
