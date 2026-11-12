namespace Tracking.Storage.Entities;

public sealed class UserRoleEntity
{
    public long UserId { get; set; }

    public long RoleId { get; set; }

    public UserEntity User { get; set; } = null!;

    public RoleEntity Role { get; set; } = null!;
}
