namespace Tracking.Storage.Entities;

public sealed class PermissionEntity
{
    public long Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public ICollection<RolePermissionEntity> RolePermissions { get; set; } = [];
}
