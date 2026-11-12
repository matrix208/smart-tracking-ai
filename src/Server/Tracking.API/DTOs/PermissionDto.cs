namespace Tracking.API.DTOs;

public sealed record PermissionDto(
    long Id,
    string Key,
    string Name,
    string Description,
    string Category);
