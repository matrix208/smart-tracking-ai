namespace Tracking.API.DTOs;

public sealed record UserSummaryDto(
    long Id,
    string Username,
    string DisplayName,
    bool IsActive,
    List<string> Roles);

public sealed record CreateUserRequest(
    string Username,
    string Password,
    string? DisplayName,
    List<long>? RoleIds);

public sealed record SetUserRolesRequest(
    List<long> RoleIds);

public sealed record SetUserActiveRequest(
    bool IsActive);
