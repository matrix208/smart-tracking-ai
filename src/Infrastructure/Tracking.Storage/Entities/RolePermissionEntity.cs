namespace Tracking.Storage.Entities;

public sealed class RolePermissionEntity
{
    public long RoleId { get; set; }

    public long PermissionId { get; set; }

    public RoleEntity Role { get; set; } = null!;

    public PermissionEntity Permission { get; set; } = null!;
}
