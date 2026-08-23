namespace Tracking.Security.Password;

public sealed class PasswordPolicy
{
   public int MinimumLength { get; init; } = 8;

    public bool RequireUppercase { get; init; } = true;

    public bool RequireLowercase { get; init; } = true;

    public bool RequireDigit { get; init; } = true;

    public bool RequireSpecialCharacter { get; init; } = true;

    public int MaximumIdenticalCharactersInRow { get; init; } = 2;

    public int MaximumSequentialCharacters { get; init; } = 2;

    public bool RejectUsername { get; init; } = true;

    public bool RejectCommonPasswords { get; init; } = true;
}
