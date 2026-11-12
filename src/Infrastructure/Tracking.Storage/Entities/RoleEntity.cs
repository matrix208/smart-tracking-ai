namespace Tracking.Storage.Entities;

public sealed class RoleEntity
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsSystemRole { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserRoleEntity> UserRoles { get; set; } = [];

    public ICollection<RolePermissionEntity> RolePermissions { get; set; } = [];
}
