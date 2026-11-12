namespace Tracking.Storage.Entities;

public sealed class UserEntity
{
    public long Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    // Legacy role field kept temporarily for backward compatibility.
    // Authorization will be driven by UserRoles.
    public string Role { get; set; } = "Administrator";

    public bool IsActive { get; set; } = true;

    public ICollection<UserRoleEntity> UserRoles { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
