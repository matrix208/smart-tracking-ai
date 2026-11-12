using Tracking.Security.Password;
using Tracking.Storage.Entities;

namespace Tracking.API.Tools;

public static class CreateAdmin
{
    public static UserEntity Create(
        string username,
        string password,
        string displayName)
    {
        var validator = new PasswordValidator();
        var validation = validator.Validate(password, username);

        if (!validation.IsValid)
        {
            throw new InvalidOperationException(
                string.Join(Environment.NewLine, validation.Errors));
        }

        var hasher = new PasswordHasher();

        return new UserEntity
        {
            Username = username,
            PasswordHash = hasher.Hash(password),
            DisplayName = displayName,
            Role = "Administrator",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}
